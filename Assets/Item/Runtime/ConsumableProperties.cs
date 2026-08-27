using System;
using UnityEngine;

namespace Worldforge.Item
{
    [Serializable]
    public sealed class ConsumableProperties
    {
        [SerializeField] private float _cooldown = 1f;
        [SerializeField] private float _consumeTime = 1f;
        [SerializeField] private bool _isReusable = false;
        [SerializeField] private float _healthRestored = 0f;
        [SerializeField] private float _staminaRestored = 0f;

        public ConsumableProperties()
        {
        }

        public ConsumableProperties(float cooldown, float consumeTime, bool isReusable, float healthRestored, float staminaRestored)
        {
            _cooldown = cooldown;
            _consumeTime = consumeTime;
            _isReusable = isReusable;
            _healthRestored = healthRestored;
            _staminaRestored = staminaRestored;
        }

        public float Cooldown
        {
            get { return _cooldown; }
        }

        public float ConsumeTime
        {
            get { return _consumeTime; }
        }

        public bool IsReusable
        {
            get { return _isReusable; }
        }

        public float HealthRestored
        {
            get { return _healthRestored; }
        }

        public float StaminaRestored
        {
            get { return _staminaRestored; }
        }

        public void Validate()
        {
            _cooldown = Mathf.Max(0f, _cooldown);
            _consumeTime = Mathf.Max(0f, _consumeTime);
            _healthRestored = Mathf.Max(0f, _healthRestored);
            _staminaRestored = Mathf.Max(0f, _staminaRestored);
        }
    }
}
