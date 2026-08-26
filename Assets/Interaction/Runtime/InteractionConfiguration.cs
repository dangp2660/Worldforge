using UnityEngine;

namespace Worldforge.Interaction
{
    [CreateAssetMenu(
        fileName = "InteractionConfiguration",
        menuName = "Worldforge/Interaction/Interaction Configuration")]
    public sealed class InteractionConfiguration : ScriptableObject
    {
        [Header("Detection")]
        [SerializeField] private float _maxDetectionDistance = 3f;
        [SerializeField] private LayerMask _detectionLayerMask = ~0;
        [SerializeField] private int _maxDetectionResults = 10;
        [SerializeField] private float _detectionInterval = 0.1f;

        [Header("Interaction")]
        [SerializeField] private float _interactionCooldown = 0.5f;

        public float MaxDetectionDistance
        {
            get { return _maxDetectionDistance; }
        }

        public LayerMask DetectionLayerMask
        {
            get { return _detectionLayerMask; }
        }

        public int MaxDetectionResults
        {
            get { return _maxDetectionResults; }
        }

        public float DetectionInterval
        {
            get { return _detectionInterval; }
        }

        public float InteractionCooldown
        {
            get { return _interactionCooldown; }
        }

        private void OnValidate()
        {
            _maxDetectionDistance = Mathf.Max(0.1f, _maxDetectionDistance);
            _maxDetectionResults = Mathf.Clamp(_maxDetectionResults, 1, 50);
            _detectionInterval = Mathf.Max(0f, _detectionInterval);
            _interactionCooldown = Mathf.Max(0f, _interactionCooldown);
        }
    }
}
