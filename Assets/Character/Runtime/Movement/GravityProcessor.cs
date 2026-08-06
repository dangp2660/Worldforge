using UnityEngine;

namespace Worldforge.Character.Movement
{
    public sealed class GravityProcessor
    {
        private float _gravityMagnitude;
        private float _terminalVelocity;
        private float _groundedGravity;
        private float _currentVerticalVelocity;

        public GravityProcessor(float gravityMagnitude, float terminalVelocity, float groundedGravity)
        {
            _gravityMagnitude = Mathf.Max(0f, gravityMagnitude);
            _terminalVelocity = Mathf.Max(0f, terminalVelocity);
            _groundedGravity = Mathf.Min(0f, groundedGravity);
        }

        public float CurrentVerticalVelocity
        {
            get { return _currentVerticalVelocity; }
        }

        public float GravityMagnitude
        {
            get { return _gravityMagnitude; }
            set { _gravityMagnitude = Mathf.Max(0f, value); }
        }

        public float TerminalVelocity
        {
            get { return _terminalVelocity; }
            set { _terminalVelocity = Mathf.Max(0f, value); }
        }

        public float GroundedGravity
        {
            get { return _groundedGravity; }
            set { _groundedGravity = Mathf.Min(0f, value); }
        }

        public float Update(bool isGrounded, float deltaTime)
        {
            if (isGrounded)
            {
                _currentVerticalVelocity = _groundedGravity;
                return _currentVerticalVelocity;
            }

            _currentVerticalVelocity -= _gravityMagnitude * deltaTime;

            if (_currentVerticalVelocity < -_terminalVelocity)
            {
                _currentVerticalVelocity = -_terminalVelocity;
            }

            return _currentVerticalVelocity;
        }

        public void Reset()
        {
            _currentVerticalVelocity = 0f;
        }
    }
}
