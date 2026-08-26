using UnityEngine;
using UnityEngine.InputSystem;
using Worldforge.Interaction.Detection;

namespace Worldforge.Interaction
{
    /// <summary>
    /// Unity Integration Boundary for the interaction system.
    /// Manages interaction input, runs distance detection via <see cref="InteractionDetector"/>,
    /// and communicates with <see cref="IInteractionService"/>.
    /// </summary>
    public sealed class InteractionBehaviour : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private InteractionConfiguration _configuration;

        [Header("Debug")]
        [SerializeField] private string _currentTargetDebug = "None";
        [SerializeField] private bool _isInteractingDebug;
        [SerializeField] private float _interactionProgressDebug;

        private RuntimeInteractionService _interactionService;
        private InteractionDetector _detector;
        private InputAction _interactAction;
        private float _detectionTimer;
        private bool _isInitialized;
        private InteractionTarget _currentTarget = InteractionTarget.None;

        public InteractionTarget CurrentTarget
        {
            get { return _currentTarget; }
        }

        public bool IsInteracting
        {
            get { return _interactionService != null && _interactionService.IsInteracting; }
        }

        internal void Initialize(RuntimeInteractionService interactionService, InteractionConfiguration configuration)
        {
            if (interactionService == null) return;

            _interactionService = interactionService;
            _configuration = configuration != null ? configuration : ScriptableObject.CreateInstance<InteractionConfiguration>();

            var bufferSize = _configuration.MaxDetectionResults > 0 ? _configuration.MaxDetectionResults : 10;
            _detector = new InteractionDetector(bufferSize);

            BindInputActions();

            _isInitialized = true;
        }

        public void Shutdown()
        {
            if (!_isInitialized) return;

            _isInitialized = false;
            ReleaseInputActions();

            _interactionService = null;
            _detector = null;
            _currentTarget = InteractionTarget.None;
        }

        private void Update()
        {
            if (!_isInitialized || _interactionService == null) return;

            UpdateDetection(Time.deltaTime);
            HandleInput();

            _interactionService.Tick(Time.deltaTime);
            UpdateDebugInfo();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void UpdateDetection(float deltaTime)
        {
            if (_configuration == null || _detector == null) return;

            _detectionTimer += deltaTime;
            if (_detectionTimer < _configuration.DetectionInterval) return;

            _detectionTimer = 0f;

            if (_interactionService.IsInteracting)
            {
                return;
            }

            _currentTarget = _detector.Detect(
                transform.position,
                _configuration.MaxDetectionDistance,
                _configuration.DetectionLayerMask);

            _interactionService.SetCurrentTarget(_currentTarget);
        }

        private void HandleInput()
        {
            if (!IsInteractInputTriggered()) return;

            if (_interactionService.IsInteracting)
            {
                _interactionService.CancelInteraction();
                return;
            }

            if (_currentTarget.HasTarget)
            {
                var result = _interactionService.RequestInteraction(gameObject, _currentTarget.Interactable);
                if (!result.IsSuccess)
                {
                    Debug.LogWarning($"[Worldforge] [Interaction] Request failed: {result.FailureReason}");
                }
            }
        }

        private bool IsInteractInputTriggered()
        {
            if (_interactAction != null)
            {
                if (_interactAction.WasPressedThisFrame() || _interactAction.triggered || _interactAction.WasPerformedThisFrame())
                {
                    return true;
                }
            }

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                return true;
            }

            if (Gamepad.current != null && (Gamepad.current.buttonNorth.wasPressedThisFrame || Gamepad.current.buttonWest.wasPressedThisFrame))
            {
                return true;
            }

            return false;
        }

        private void UpdateDebugInfo()
        {
            _currentTargetDebug = _currentTarget.HasTarget ? _currentTarget.ToString() : "None";
            _isInteractingDebug = _interactionService.IsInteracting;
            _interactionProgressDebug = _interactionService.InteractionProgress;
        }

        private void BindInputActions()
        {
            var playerMap = InputSystem.actions?.FindActionMap("Player");
            if (playerMap != null)
            {
                _interactAction = playerMap.FindAction("Interact");
                _interactAction?.Enable();
            }
        }

        private void ReleaseInputActions()
        {
            _interactAction = null;
        }
    }
}
