using UnityEngine;

namespace Worldforge.Infrastructure.Cameras
{
    public sealed class CameraImpulseProcessor
    {
        private Vector3 _currentImpulse;
        private Vector3 _velocity;
        private float _duration;
        private float _elapsed;

        public Vector3 CurrentImpulse
        {
            get { return _currentImpulse; }
        }

        public bool IsActive
        {
            get { return _elapsed < _duration || _currentImpulse.sqrMagnitude > 0.0001f; }
        }

        public void AddImpulse(Vector3 impulse, float duration)
        {
            _currentImpulse += impulse;
            _duration = Mathf.Max(_duration - _elapsed, Mathf.Max(0.01f, duration));
            _elapsed = 0f;
        }

        public void AddShake(float intensity, float duration)
        {
            var randomVector = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)).normalized * intensity;

            AddImpulse(randomVector, duration);
        }

        public Vector3 Evaluate(float deltaTime)
        {
            if (!IsActive)
            {
                _currentImpulse = Vector3.zero;
                _velocity = Vector3.zero;
                return Vector3.zero;
            }

            _elapsed += deltaTime;
            var dt = Mathf.Max(0.0001f, deltaTime);
            var dampTime = Mathf.Max(0.01f, _duration - _elapsed);

            _currentImpulse = Vector3.SmoothDamp(
                _currentImpulse,
                Vector3.zero,
                ref _velocity,
                dampTime,
                float.PositiveInfinity,
                dt);

            return _currentImpulse;
        }

        public void Reset()
        {
            _currentImpulse = Vector3.zero;
            _velocity = Vector3.zero;
            _duration = 0f;
            _elapsed = 0f;
        }
    }
}
