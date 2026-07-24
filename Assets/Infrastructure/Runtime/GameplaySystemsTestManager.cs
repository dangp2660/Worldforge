using UnityEngine;
using Worldforge.Core.Bootstrap;
using Worldforge.Core.Services;
using Worldforge.Gathering.Services;
using Worldforge.Inventory.Services;

namespace Worldforge.Infrastructure.Development
{
    public sealed class GameplaySystemsTestManager : MonoBehaviour
    {
        [SerializeField] private string primaryInventoryContainerId = "dev.inventory.primary";
        [SerializeField] private string secondaryInventoryContainerId = "dev.inventory.gathering";
        [SerializeField] private string testGatherNodeId = "dev.gathering.node";

        private void Start()
        {
            if (!BootstrapManager.HasInstance)
            {
                Debug.LogWarning("[Worldforge] [Warning] [Development.GameplayTest] BootstrapManager is not available.");
                return;
            }

            if (!BootstrapManager.TryResolve<IInventoryService>(out var inventoryService) || inventoryService == null)
            {
                Debug.LogWarning("[Worldforge] [Warning] [Development.GameplayTest] Inventory service is not available.");
                return;
            }

            if (!BootstrapManager.TryResolve<IGatheringService>(out var gatheringService) || gatheringService == null)
            {
                Debug.LogWarning("[Worldforge] [Warning] [Development.GameplayTest] Gathering service is not available.");
                return;
            }

            inventoryService.RegisterContainer(primaryInventoryContainerId);
            inventoryService.RegisterContainer(secondaryInventoryContainerId);

            var canGather = gatheringService.CanGather(testGatherNodeId);

            if (BootstrapManager.TryResolve<ILogService>(out var logger) && logger != null)
            {
                logger.Info(
                    "Development.GameplayTest",
                    $"Inventory containers registered: {inventoryService.RegisteredContainerCount}. " +
                    $"Gather check for '{testGatherNodeId}' returned {canGather}.");
                return;
            }

            Debug.Log(
                $"[Worldforge] [Info] [Development.GameplayTest] Inventory containers registered: " +
                $"{inventoryService.RegisteredContainerCount}. Gather check for '{testGatherNodeId}' returned {canGather}.");
        }
    }
}
