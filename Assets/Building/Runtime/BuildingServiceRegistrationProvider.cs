using System;
using UnityEngine;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;

namespace Worldforge.Building
{
    // Bootstrap provider that registers building services into the Core DI container.
    public sealed class BuildingServiceRegistrationProvider : IServiceRegistrationProvider
    {
        private const string ConfigurationResourcePath = "BuildingPlacementConfiguration";
        private const string SubfolderConfigurationResourcePath = "Building/BuildingPlacementConfiguration";

        public int Order
        {
            get { return 140; }
        }

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            services.AddSingleton<IBuildingPlacementService>(resolver =>
            {
                var logger = resolver.TryResolve<ILogService>(out var resolvedLogger)
                    ? resolvedLogger
                    : null;

                var configuration = Resources.Load<BuildingPlacementConfiguration>(ConfigurationResourcePath);
                if (configuration == null)
                {
                    configuration = Resources.Load<BuildingPlacementConfiguration>(SubfolderConfigurationResourcePath);
                }

                if (configuration == null)
                {
                    var allConfigs = Resources.FindObjectsOfTypeAll<BuildingPlacementConfiguration>();
                    if (allConfigs != null && allConfigs.Length > 0)
                    {
                        configuration = allConfigs[0];
                    }
                }

                if (configuration == null)
                {
                    logger?.Warning(
                        "Building.Placement",
                        $"BuildingPlacementConfiguration asset not found at Resources/{ConfigurationResourcePath}. Using runtime default instance.");
                    configuration = ScriptableObject.CreateInstance<BuildingPlacementConfiguration>();
                }
                else
                {
                    logger?.Info(
                        "Building.Placement",
                        "Building placement configuration loaded successfully.");
                }

                logger?.Info("Building.Placement", "Building placement service registered.");
                return new RuntimeBuildingPlacementService(configuration, logger);
            });
        }
    }
}
