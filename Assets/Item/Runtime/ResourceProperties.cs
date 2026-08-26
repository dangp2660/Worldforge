using System;
using UnityEngine;

namespace Worldforge.Item
{
    [Serializable]
    public sealed class ResourceProperties
    {
        [SerializeField] private float _gatherTime = 2f;
        [SerializeField] private float _respawnTime = 60f;
        [SerializeField] private float _hardness = 1f;
        [SerializeField] private ToolType _requiredToolType = ToolType.None;

        public ResourceProperties()
        {
        }

        public ResourceProperties(float gatherTime, float respawnTime, float hardness, ToolType requiredToolType)
        {
            _gatherTime = gatherTime;
            _respawnTime = respawnTime;
            _hardness = hardness;
            _requiredToolType = requiredToolType;
        }

        public float GatherTime
        {
            get { return _gatherTime; }
        }

        public float RespawnTime
        {
            get { return _respawnTime; }
        }

        public float Hardness
        {
            get { return _hardness; }
        }

        public ToolType RequiredToolType
        {
            get { return _requiredToolType; }
        }

        public void Validate()
        {
            _gatherTime = Mathf.Max(0.1f, _gatherTime);
            _respawnTime = Mathf.Max(0f, _respawnTime);
            _hardness = Mathf.Max(0f, _hardness);
        }
    }
}
