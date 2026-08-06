using UnityEngine;

namespace Worldforge.Character.Movement
{
    public readonly struct MovementFrameInput
    {
        public MovementFrameInput(
            Vector2 rawInput,
            bool isSprinting,
            Transform cameraTransform,
            Vector3 characterPosition,
            float characterRadius,
            float deltaTime)
        {
            RawInput = rawInput;
            IsSprinting = isSprinting;
            CameraTransform = cameraTransform;
            CharacterPosition = characterPosition;
            CharacterRadius = characterRadius;
            DeltaTime = deltaTime;
        }

        public Vector2 RawInput { get; }

        public bool IsSprinting { get; }

        public Transform CameraTransform { get; }

        public Vector3 CharacterPosition { get; }

        public float CharacterRadius { get; }

        public float DeltaTime { get; }
    }
}
