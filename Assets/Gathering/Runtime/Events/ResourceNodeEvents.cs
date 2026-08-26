using UnityEngine;

namespace Worldforge.Gathering
{
    public readonly struct ResourceNodeStateChangedEvent
    {
        public ResourceNodeBehaviour Node { get; }
        public ResourceNodeState PreviousState { get; }
        public ResourceNodeState NewState { get; }
        public float Timestamp { get; }

        public ResourceNodeStateChangedEvent(
            ResourceNodeBehaviour node,
            ResourceNodeState previousState,
            ResourceNodeState newState,
            float timestamp)
        {
            Node = node;
            PreviousState = previousState;
            NewState = newState;
            Timestamp = timestamp;
        }
    }

    public readonly struct ResourceNodeGatheredEvent
    {
        public ResourceNodeBehaviour Node { get; }
        public GameObject Interactor { get; }
        public GatheringHarvestResult HarvestResult { get; }
        public float Timestamp { get; }

        public ResourceNodeGatheredEvent(
            ResourceNodeBehaviour node,
            GameObject interactor,
            GatheringHarvestResult harvestResult,
            float timestamp)
        {
            Node = node;
            Interactor = interactor;
            HarvestResult = harvestResult;
            Timestamp = timestamp;
        }
    }

    public readonly struct ResourceNodeDepletedEvent
    {
        public ResourceNodeBehaviour Node { get; }
        public GameObject Interactor { get; }
        public float RespawnDuration { get; }
        public float Timestamp { get; }

        public ResourceNodeDepletedEvent(
            ResourceNodeBehaviour node,
            GameObject interactor,
            float respawnDuration,
            float timestamp)
        {
            Node = node;
            Interactor = interactor;
            RespawnDuration = respawnDuration;
            Timestamp = timestamp;
        }
    }

    public readonly struct ResourceNodeRespawnedEvent
    {
        public ResourceNodeBehaviour Node { get; }
        public float Timestamp { get; }

        public ResourceNodeRespawnedEvent(ResourceNodeBehaviour node, float timestamp)
        {
            Node = node;
            Timestamp = timestamp;
        }
    }
}
