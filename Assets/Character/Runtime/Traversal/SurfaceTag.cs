using UnityEngine;

namespace Worldforge.Character.Traversal
{
    public sealed class SurfaceTag : MonoBehaviour
    {
        [SerializeField] private SurfaceType _surfaceType = SurfaceType.Default;

        public SurfaceType SurfaceType
        {
            get { return _surfaceType; }
            set { _surfaceType = value; }
        }
    }
}
