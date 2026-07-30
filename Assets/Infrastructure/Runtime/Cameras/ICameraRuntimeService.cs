using UnityEngine;
using UnityEngine.SceneManagement;

namespace Worldforge.Infrastructure.Cameras
{
    public interface ICameraRuntimeService
    {
        Camera ActiveCamera { get; }

        Transform FollowTarget { get; }

        bool IsPrepared { get; }

        void PrepareForScene(Scene scene);

        void BindToTarget(Transform target);

        void ClearTarget();

        void ApplyConfiguration(CameraFollowConfiguration configuration);
    }
}