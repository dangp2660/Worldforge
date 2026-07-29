using NUnit.Framework;
using UnityEngine;
using Worldforge.Core.Bootstrap;
using Worldforge.Infrastructure.Cameras;

namespace Worldforge.Infrastructure.Tests
{
    public class CameraRuntimeBootstrapTests
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

            var runtimeCamera = GameObject.Find("Worldforge.RuntimeCamera");
            if (runtimeCamera != null)
            {
                Object.DestroyImmediate(runtimeCamera);
            }
        }

        [Test]
        public void CreateDefault_RegistersAndPreparesRuntimeCamera()
        {
            var flow = ApplicationStartupFlow.CreateDefault();
            var context = CreateContext();

            flow.Initialize(context);

            var cameraRuntimeService = context.Services.Resolve<ICameraRuntimeService>();

            Assert.NotNull(cameraRuntimeService);
            CollectionAssert.Contains(context.LoadedSystems, "Infrastructure.Camera");
            Assert.IsTrue(cameraRuntimeService.IsPrepared);
            Assert.NotNull(cameraRuntimeService.ActiveCamera);
            Assert.NotNull(cameraRuntimeService.ActiveCamera.GetComponent<RuntimeCameraController>());

            flow.Shutdown("TestCleanup");
        }

        private ApplicationBootstrapContext CreateContext()
        {
            managerObject = new GameObject("Infrastructure.Camera.Tests");
            var manager = managerObject.AddComponent<BootstrapManager>();
            return new ApplicationBootstrapContext(manager);
        }
    }
}
