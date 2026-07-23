using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;
using Worldforge.Gathering.Services;
using Worldforge.Inventory.Services;

namespace Worldforge.Core.Tests
{
    public class ApplicationStartupFlowTests
    {
        private GameObject managerObject;

        [TearDown]
        public void TearDown()
        {
            if (managerObject != null)
            {
                UnityEngine.Object.DestroyImmediate(managerObject);
                managerObject = null;
            }
        }

        [Test]
        public void Initialize_LoadsSystemsInDependencyOrder()
        {
            var events = new List<string>();
            var flow = new ApplicationStartupFlow(
                new RecordingSystem("SceneFlow", events, dependencies: new[] { "Input" }),
                new RecordingSystem("Gameplay.Inventory", events, dependencies: new[] { "SceneFlow" }, category: ApplicationSystemCategory.Gameplay),
                new RecordingSystem("Input", events));
            var context = CreateContext();

            flow.Initialize(context);

            CollectionAssert.AreEqual(
                new[] { "initialize:Input", "initialize:SceneFlow", "initialize:Gameplay.Inventory" },
                events);
            CollectionAssert.AreEqual(
                new[] { "Input", "SceneFlow", "Gameplay.Inventory" },
                context.LoadedSystems);
            CollectionAssert.AreEqual(
                new[] { "Gameplay.Inventory" },
                context.LoadedGameplayModules);
        }

        [Test]
        public void Shutdown_UnloadsSystemsInReverseOrder()
        {
            var events = new List<string>();
            var flow = new ApplicationStartupFlow(
                new RecordingSystem("Input", events),
                new RecordingSystem("SceneFlow", events));
            var context = CreateContext();

            flow.Initialize(context);
            flow.Shutdown();

            CollectionAssert.AreEqual(
                new[]
                {
                    "initialize:Input",
                    "initialize:SceneFlow",
                    "shutdown:SceneFlow",
                    "shutdown:Input"
                },
                events);
        }

        [Test]
        public void Initialize_IgnoresDuplicateSystemsByName()
        {
            var events = new List<string>();
            var flow = new ApplicationStartupFlow(
                new RecordingSystem("Input", events),
                new RecordingSystem("Input", events));
            var context = CreateContext();

            flow.Initialize(context);
            flow.Shutdown();

            CollectionAssert.AreEqual(
                new[]
                {
                    "initialize:Input",
                    "shutdown:Input"
                },
                events);
            CollectionAssert.AreEqual(new[] { "Input" }, context.LoadedSystems);
        }

        [Test]
        public void Initialize_ThrowsWhenDependencyIsMissing()
        {
            var flow = new ApplicationStartupFlow(
                new RecordingSystem("Gameplay.Gathering", new List<string>(), dependencies: new[] { "Gameplay.Inventory" }, category: ApplicationSystemCategory.Gameplay));
            var context = CreateContext();

            var exception = Assert.Throws<InvalidOperationException>(() => flow.Initialize(context));

            StringAssert.Contains("Gameplay.Inventory", exception.Message);
        }

        [Test]
        public void CreateDefault_InitializesCoreAndGameplayModules()
        {
            var flow = ApplicationStartupFlow.CreateDefault();
            var context = CreateContext();

            flow.Initialize(context);

            CollectionAssert.AreEqual(
                new[]
                {
                    "Input",
                    "SceneFlow",
                    "Gameplay.Inventory",
                    "Gameplay.Gathering"
                },
                context.LoadedSystems);
            CollectionAssert.AreEqual(
                new[]
                {
                    "Gameplay.Inventory",
                    "Gameplay.Gathering"
                },
                context.LoadedGameplayModules);
        }

        [Test]
        public void Initialize_RegistersCoreAndGameplayServices()
        {
            var flow = new ApplicationStartupFlow();
            var context = CreateContext();

            flow.Initialize(context);

            Assert.NotNull(context.Services);
            Assert.NotNull(context.Services.Resolve<IApplicationInfoService>());
            Assert.NotNull(context.Services.Resolve<IClockService>());
            Assert.NotNull(context.Services.Resolve<IInputActionsService>());
            Assert.NotNull(context.Services.Resolve<IInventoryService>());
            Assert.NotNull(context.Services.Resolve<IGatheringService>());
        }

        [Test]
        public void Services_FollowConfiguredLifetimeRules()
        {
            var flow = new ApplicationStartupFlow();
            var context = CreateContext();

            flow.Initialize(context);

            var resolver = context.Services;

            var inventoryA = resolver.Resolve<IInventoryService>();
            var inventoryB = resolver.Resolve<IInventoryService>();
            Assert.AreSame(inventoryA, inventoryB);

            var gatheringA = resolver.Resolve<IGatheringService>();
            var gatheringB = resolver.Resolve<IGatheringService>();
            Assert.AreNotSame(gatheringA, gatheringB);

            var rootSessionA = resolver.Resolve<IInventorySessionService>();
            var rootSessionB = resolver.Resolve<IInventorySessionService>();
            Assert.AreSame(rootSessionA, rootSessionB);

            using var scopeA = resolver.CreateScope();
            using var scopeB = resolver.CreateScope();

            var sessionA1 = scopeA.Resolve<IInventorySessionService>();
            var sessionA2 = scopeA.Resolve<IInventorySessionService>();
            var sessionB = scopeB.Resolve<IInventorySessionService>();

            Assert.AreSame(sessionA1, sessionA2);
            Assert.AreNotSame(sessionA1, sessionB);
            Assert.AreNotSame(rootSessionA, sessionA1);
        }

        [Test]
        public void BootstrapManager_CanResolveServicesAfterInitialization()
        {
            managerObject = new GameObject("BootstrapManagerTests");
            var manager = managerObject.AddComponent<BootstrapManager>();

            manager.Initialize(new ApplicationStartupFlow());

            var inventoryService = manager.Resolve<IInventoryService>();
            var resolvedFromStatic = BootstrapManager.ResolveRequired<IInventoryService>();

            Assert.NotNull(inventoryService);
            Assert.AreSame(inventoryService, resolvedFromStatic);
        }

        private ApplicationBootstrapContext CreateContext()
        {
            managerObject = new GameObject("BootstrapManagerTests");
            var manager = managerObject.AddComponent<BootstrapManager>();
            return new ApplicationBootstrapContext(manager);
        }

        private sealed class RecordingSystem : IApplicationSystem
        {
            private readonly IList<string> events;
            private readonly IReadOnlyList<string> dependencies;

            public RecordingSystem(
                string name,
                IList<string> events,
                IReadOnlyList<string> dependencies = null,
                ApplicationSystemCategory category = ApplicationSystemCategory.Core,
                int order = 0)
            {
                Name = name;
                this.events = events;
                this.dependencies = dependencies ?? Array.Empty<string>();
                Category = category;
                Order = order;
            }

            public string Name { get; }

            public int Order { get; }

            public ApplicationSystemCategory Category { get; }

            public IReadOnlyList<string> Dependencies
            {
                get { return dependencies; }
            }

            public void Initialize(ApplicationBootstrapContext context)
            {
                events.Add("initialize:" + Name);
            }

            public void Shutdown(ApplicationBootstrapContext context)
            {
                events.Add("shutdown:" + Name);
            }
        }
    }
}
