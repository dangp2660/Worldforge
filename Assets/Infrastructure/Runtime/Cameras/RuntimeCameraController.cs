using System;
using UnityEngine;

namespace Worldforge.Infrastructure.Cameras
{
    [DisallowMultipleComponent]
    public sealed class RuntimeCameraController : MonoBehaviour
    {
        [SerializeField] private Vector3 followOffset = new(0f, 8f, -6f);
        [SerializeField] private Vector3 lookAtOffset = new(0f, 1.5f, 0f);
        [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.12f;
        [SerializeField, Min(0f)] private float rotationLerpSpeed = 12f;
        [SerializeField] private bool snapOnTargetAcquire = true;

        private Func<Transform> targetProvider;
        private Transform followTarget;
        private Vector3 positionVelocity;
        private bool hasAppliedInitialPose;

        public Transform FollowTarget
        {
            get { return followTarget; }
        }

        public void SetTargetProvider(Func<Transform> provider)
        {
            targetProvider = provider;
            RefreshFollowTarget();
        }

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
            positionVelocity = Vector3.zero;
            hasAppliedInitialPose = false;
        }

        public void ClearFollowTarget()
        {
            targetProvider = null;
            followTarget = null;
            positionVelocity = Vector3.zero;
            hasAppliedInitialPose = false;
        }

        private void LateUpdate()
        {
            if (followTarget == null || !followTarget.gameObject.activeInHierarchy)
            {
                RefreshFollowTarget();
            }

            if (followTarget == null)
            {
                return;
            }

            var focusPoint = followTarget.position + lookAtOffset;
            var desiredPosition = focusPoint + followOffset;
            var desiredRotation = Quaternion.LookRotation(focusPoint - desiredPosition, Vector3.up);

            if (snapOnTargetAcquire && !hasAppliedInitialPose)
            {
                transform.SetPositionAndRotation(desiredPosition, desiredRotation);
                hasAppliedInitialPose = true;
                return;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                positionSmoothTime);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                rotationLerpSpeed * Time.unscaledDeltaTime);
        }

        private void RefreshFollowTarget()
        {
            if (targetProvider == null)
            {
                return;
            }

            var resolvedTarget = targetProvider();
            if (resolvedTarget == followTarget)
            {
                return;
            }

            SetFollowTarget(resolvedTarget);
        }
    }
}
