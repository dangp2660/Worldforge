using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Worldforge.Core.Bootstrap;

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
