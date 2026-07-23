using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Worldforge.Core.Services
{
    public interface IApplicationInfoService
    {
        IReadOnlyList<string> LoadedSystems { get; }

        IReadOnlyList<string> LoadedGameplayModules { get; }

        string StartupScenePath { get; }

        string ActiveScenePath { get; }
    }

    public interface IClockService
    {
        float TimeSinceStartup { get; }

        float UnscaledTimeSinceStartup { get; }
    }

    public interface IInputActionsService
    {
        InputActionAsset Actions { get; }
    }
}

namespace Worldforge.Core.Bootstrap
{
    using Worldforge.Core.Services;

    internal sealed class CoreServiceRegistrationProvider : IServiceRegistrationProvider
    {
        public int Order
        {
            get { return 0; }
        }

        public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
        {
            services.AddSingleton(context.Manager);
            services.AddSingleton(context);
            services.AddSingleton<IApplicationInfoService>(_ => new ApplicationInfoService(context));
            services.AddSingleton<IClockService>(_ => new UnityClockService());
            services.AddSingleton<IInputActionsService>(_ => new InputActionsService(context));
        }
    }

    internal sealed class ApplicationInfoService : IApplicationInfoService
    {
        private readonly ApplicationBootstrapContext context;

        public ApplicationInfoService(ApplicationBootstrapContext context)
        {
            this.context = context;
        }

        public IReadOnlyList<string> LoadedSystems
        {
            get { return context.LoadedSystems; }
        }

        public IReadOnlyList<string> LoadedGameplayModules
        {
            get { return context.LoadedGameplayModules; }
        }

        public string StartupScenePath
        {
            get { return context.StartupScenePath; }
        }

        public string ActiveScenePath
        {
            get { return context.ActiveScenePath; }
        }
    }

    internal sealed class UnityClockService : IClockService
    {
        public float TimeSinceStartup
        {
            get { return Time.time; }
        }

        public float UnscaledTimeSinceStartup
        {
            get { return Time.unscaledTime; }
        }
    }

    internal sealed class InputActionsService : IInputActionsService
    {
        private readonly ApplicationBootstrapContext context;

        public InputActionsService(ApplicationBootstrapContext context)
        {
            this.context = context;
        }

        public InputActionAsset Actions
        {
            get { return context.ProjectWideInputActions; }
        }
    }
}
