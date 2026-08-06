using UnityEngine;
using UnityEngine.SceneManagement;

namespace Worldforge.Infrastructure.Cameras
{
    public interface ICameraRuntimeService
    {
        Camera ActiveCamera { get; }

        Transform FollowTarget { get; }

        Transform SecondaryTarget { get; }

        CameraMode CurrentMode { get; }

        Vector3 TargetOffset { get; }

        Vector3 CameraForward { get; }

        Vector3 CameraRight { get; }

        bool IsPrepared { get; }

        void PrepareForScene(Scene scene);

        void BindToTarget(Transform target);

        void ClearTarget();

        void SetMode(CameraMode mode, Transform secondaryTarget = null);

        void SetTargetOffset(Vector3 offset);

        void ResetTargetOffset();

        void AddImpulse(Vector3 impulse, float duration);

        void AddShake(float intensity, float duration);

        void SetFieldOfView(float fieldOfView);

        void ResetFieldOfView();

        Vector3 GetCameraRelativeDirection(Vector2 inputDirection);

        void ApplyConfiguration(CameraFollowConfiguration configuration);

        void SetOrbitAngles(float pitch, float yaw);

        void SetZoomDistance(float distance);

        void ResetPose();

        void SetCursorLock(bool isLocked);
    }
}