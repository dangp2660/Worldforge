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

            flow.Shutdown("TestCleanup");
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

        [Test]
        public void Shutdown_ExecutesSaveCleanupReleaseAndDisposeInOrder()
        {
            ShutdownTrackingState.Reset();

            var flow = new ApplicationStartupFlow(new ShutdownTrackingSystem());
            var context = CreateContext();

            flow.Initialize(context);

            var snapshotStore = context.Services.Resolve<IApplicationShutdownSnapshotStore>();

            flow.Shutdown("TestShutdown");
            ShutdownTrackingEventSource.Raise();

            CollectionAssert.AreEqual(
                new[]
                {
                    "initialize:Test.ShutdownTracking",
                    "save:Test.ShutdownTracking",
                    "shutdown:Test.ShutdownTracking",
                    "cleanup:Test.ShutdownTracking.Subscription",
                    "dispose:Test.ShutdownTracking.Resource",
                    "destroy:Test.ShutdownTracking.Temp",
                    "dispose:Test.ShutdownTracking.Service"
                },
                ShutdownTrackingState.Events);
            Assert.Zero(ShutdownTrackingState.SignalHandlerInvocations);
            Assert.IsTrue(ShutdownTrackingState.WasTemporaryObjectDestroyed);
            Assert.NotNull(snapshotStore.LastSavedSnapshot);
            Assert.AreEqual("TestShutdown", snapshotStore.LastSavedSnapshot.shutdownReason);
            Assert.NotNull(snapshotStore.LastSavedSnapshot.runtimeData);
            Assert.That(
                Array.Exists(
                    snapshotStore.LastSavedSnapshot.runtimeData,
                    entry => entry != null &&
                             entry.key == "test.shutdownTrackingServiceState" &&
                             entry.value == "alive"),
                Is.True);
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

        private interface IShutdownTrackingService
        {
            string State { get; }
        }

        private sealed class ShutdownTrackingService : IShutdownTrackingService, IDisposable
        {
            private bool disposed;

            public string State
            {
                get { return disposed ? "disposed" : "alive"; }
            }

            public void Dispose()
            {
                disposed = true;
                ShutdownTrackingState.Events.Add("dispose:Test.ShutdownTracking.Service");
            }
        }

        private sealed class ShutdownTrackingServiceRegistrationProvider : IServiceRegistrationProvider
        {
            public int Order
            {
                get { return 1000; }
            }

            public void RegisterServices(ApplicationBootstrapContext context, IServiceRegistry services)
            {
                services.AddSingleton<IShutdownTrackingService>(_ => new ShutdownTrackingService());
            }
        }

        private sealed class ShutdownTrackingSystem : IApplicationSystem
        {
            public string Name
            {
                get { return "Test.ShutdownTracking"; }
            }

            public int Order
            {
                get { return 500; }
            }

            public ApplicationSystemCategory Category
            {
                get { return ApplicationSystemCategory.Core; }
            }

            public IReadOnlyList<string> Dependencies
            {
                get { return Array.Empty<string>(); }
            }

            public void Initialize(ApplicationBootstrapContext context)
            {
                ShutdownTrackingState.Events.Add("initialize:Test.ShutdownTracking");
                context.Services.Resolve<IShutdownTrackingService>();

                var temporaryObject = new GameObject("ShutdownTracking.Temp");
                temporaryObject.AddComponent<ShutdownTrackingTemporaryObjectTracker>();
                context.RegisterTemporaryObject("Test.ShutdownTracking.Temp", temporaryObject);

                ShutdownTrackingEventSource.Signal += ShutdownTrackingState.OnSignal;
                context.RegisterEventSubscription(
                    "Test.ShutdownTracking.Subscription",
                    () =>
                    {
                        ShutdownTrackingState.Events.Add("cleanup:Test.ShutdownTracking.Subscription");
                        ShutdownTrackingEventSource.Signal -= ShutdownTrackingState.OnSignal;
                    },
                    10);

                context.RegisterSaveOperation(
                    "Test.ShutdownTracking.Save",
                    currentContext =>
                    {
                        ShutdownTrackingState.Events.Add("save:Test.ShutdownTracking");
                        var trackingService = currentContext.Services.Resolve<IShutdownTrackingService>();
                        currentContext.RecordRuntimeState("test.shutdownTrackingServiceState", trackingService.State);
                    },
                    10);

                context.RegisterRuntimeResource(
                    "Test.ShutdownTracking.Resource",
                    new TrackingDisposableResource());
            }

            public void Shutdown(ApplicationBootstrapContext context)
            {
                ShutdownTrackingState.Events.Add("shutdown:Test.ShutdownTracking");
            }
        }

        private sealed class TrackingDisposableResource : IDisposable
        {
            public void Dispose()
            {
                ShutdownTrackingState.Events.Add("dispose:Test.ShutdownTracking.Resource");
            }
        }

        private sealed class ShutdownTrackingTemporaryObjectTracker : MonoBehaviour
        {
            private void OnDestroy()
            {
                ShutdownTrackingState.Events.Add("destroy:Test.ShutdownTracking.Temp");
                ShutdownTrackingState.WasTemporaryObjectDestroyed = true;
            }
        }

        private static class ShutdownTrackingEventSource
        {
            public static event Action Signal;

            public static void Raise()
            {
                Signal?.Invoke();
            }

            public static void Reset()
            {
                Signal = null;
            }
        }

        private static class ShutdownTrackingState
        {
            public static List<string> Events { get; } = new List<string>();

            public static int SignalHandlerInvocations { get; private set; }

            public static bool WasTemporaryObjectDestroyed { get; set; }

            public static void Reset()
            {
                Events.Clear();
                SignalHandlerInvocations = 0;
                WasTemporaryObjectDestroyed = false;
                ShutdownTrackingEventSource.Reset();
            }

            public static void OnSignal()
            {
                SignalHandlerInvocations++;
            }
        }
    }
}
