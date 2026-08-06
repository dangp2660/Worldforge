using UnityEngine;

namespace Worldforge.Infrastructure.Cameras
{
    public readonly struct CameraInputResult
    {
        public CameraInputResult(float yawDelta, float pitchDelta, float zoomDelta)
        {
            YawDelta = yawDelta;
            PitchDelta = pitchDelta;
            ZoomDelta = zoomDelta;
        }

        public float YawDelta { get; }

        public float PitchDelta { get; }

        public float ZoomDelta { get; }

        public bool HasInput
        {
            get
            {
                return Mathf.Abs(YawDelta) > 0.0001f
                    || Mathf.Abs(PitchDelta) > 0.0001f
                    || Mathf.Abs(ZoomDelta) > 0.0001f;
            }
        }
    }
}
