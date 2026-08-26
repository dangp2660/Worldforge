using UnityEngine;

namespace Worldforge.Interaction
{
    /// <summary>
    /// Contract for world objects that can be interacted with.
    /// Implemented by MonoBehaviours placed on scene objects (resource nodes, NPCs, crafting stations, etc.).
    /// </summary>
    public interface IInteractable
    {
        InteractionType Type { get; }

        // Display text for the interaction prompt UI (e.g. "Press E to gather").
        string InteractionPrompt { get; }

        // Duration in seconds. 0 = instant interaction.
        // Aligns with GatherTime, CraftTime, etc. from schema.
        float InteractionDuration { get; }

        // Whether this object currently accepts interactions.
        // False when on cooldown, depleted, quest-locked, etc.
        bool IsInteractable { get; }

        // World position used for range calculation.
        Transform InteractionPoint { get; }

        void OnInteractionStarted(InteractionContext context);

        void OnInteractionCompleted(InteractionContext context);

        void OnInteractionCancelled(InteractionContext context);
    }
}
