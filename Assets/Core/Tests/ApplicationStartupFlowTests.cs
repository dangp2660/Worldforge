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
                Object.DestroyImmediate(managerObject);
                managerObject = null;
            }
        }

        [Test]
        public void Initialize_LoadsSystemsInDeclaredOrder()
        {
            var events = new List<string>();
            var flow = new ApplicationStartupFlow(
                new RecordingSystem("Input", events),
                new RecordingSystem("SceneFlow", events));
            var context = CreateContext();

            flow.Initialize(context);

            CollectionAssert.AreEqual(
                new[] { "initialize:Input", "initialize:SceneFlow" },
                events);
            CollectionAssert.AreEqual(
                new[] { "Input", "SceneFlow" },
                context.LoadedSystems);
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

            using (var scopeA = resolver.CreateScope())
            using (var scopeB = resolver.CreateScope())
            {
                var sessionA1 = scopeA.Resolve<IInventorySessionService>();
                var sessionA2 = scopeA.Resolve<IInventorySessionService>();
                var sessionB = scopeB.Resolve<IInventorySessionService>();

                Assert.AreSame(sessionA1, sessionA2);
                Assert.AreNotSame(sessionA1, sessionB);
                Assert.AreNotSame(rootSessionA, sessionA1);
            }
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

            public RecordingSystem(string name, IList<string> events)
            {
                Name = name;
                this.events = events;
            }

            public string Name { get; }

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
