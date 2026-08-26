using UnityEngine;

namespace Worldforge.Interaction
{
    /// <summary>
    /// Base MonoBehaviour for interactable objects in the scene.
    /// Subclass for specific interaction types (e.g. ResourceNodeInteractable, NPCInteractable).
    /// Requires a Collider on the same GameObject for detection.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableBehaviour : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private InteractionType _interactionType = InteractionType.Use;
        [SerializeField] private string _interactionPrompt = "Interact";
        [SerializeField] private float _interactionDuration;

        [Header("State")]
        [SerializeField] private bool _isInteractable = true;

        public virtual InteractionType Type
        {
            get { return _interactionType; }
        }

        public virtual string InteractionPrompt
        {
            get { return _interactionPrompt; }
        }

        public virtual float InteractionDuration
        {
            get { return _interactionDuration; }
        }

        public virtual bool IsInteractable
        {
            get { return _isInteractable && isActiveAndEnabled; }
        }

        public virtual Transform InteractionPoint
        {
            get { return transform; }
        }

        public virtual void OnInteractionStarted(InteractionContext context)
        {
        }

        public virtual void OnInteractionCompleted(InteractionContext context)
        {
        }

        public virtual void OnInteractionCancelled(InteractionContext context)
        {
        }

        // Allow subclasses to control interactable state at runtime.
        protected void SetInteractable(bool value)
        {
            _isInteractable = value;
        }

        private void OnValidate()
        {
            _interactionDuration = Mathf.Max(0f, _interactionDuration);
        }
    }
}
