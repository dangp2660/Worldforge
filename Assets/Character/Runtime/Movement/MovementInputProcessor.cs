using UnityEngine;

namespace Worldforge.Character.Movement
{
    public sealed class MovementInputProcessor
    {
        private float _deadZone;

        public MovementInputProcessor(float deadZone)
        {
            _deadZone = Mathf.Clamp01(deadZone);
        }

        public float DeadZone
        {
            get { return _deadZone; }
            set { _deadZone = Mathf.Clamp01(value); }
        }

        public Vector3 ProcessInput(Vector2 rawInput, Transform cameraTransform)
        {
            if (rawInput.sqrMagnitude < _deadZone * _deadZone)
            {
                return Vector3.zero;
            }

            var input = Vector2.ClampMagnitude(rawInput, 1f);

            if (cameraTransform == null)
            {
                return new Vector3(input.x, 0f, input.y);
            }

            var cameraForward = cameraTransform.forward;
            var cameraRight = cameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            if (cameraForward.sqrMagnitude < 0.001f)
            {
                cameraForward = Vector3.forward;
            }

            if (cameraRight.sqrMagnitude < 0.001f)
            {
                cameraRight = Vector3.right;
            }

            var worldDirection = cameraRight * input.x + cameraForward * input.y;

            if (worldDirection.sqrMagnitude > 1f)
            {
                worldDirection.Normalize();
            }

            return worldDirection;
        }
    }
}
