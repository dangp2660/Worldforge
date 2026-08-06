using UnityEngine;

namespace Worldforge.Core.Bootstrap
{
    public sealed class GameSessionSpawnPoint : MonoBehaviour
    {
        [SerializeField] private int priority;

        public int Priority
        {
            get { return priority; }
        }
    }
}
