using UnityEngine;

namespace tdk.Boids
{
    public class BoidFlockManager : MonoBehaviour
    {
        [SerializeField]
        Transform target;
        [SerializeField]
        BoidSettings settings;
        BoidFlock flock;

        void Awake()
        {
            flock = new(settings)
            {
                Target = target
            };
        }

        [ContextMenu("AddBoid")]
        public void AddBoid()
        {
            flock.Add(transform);
        }

        void Update()
        {
            flock.UpdateBoids();
        }

        void OnDrawGizmos()
        {
            if (flock == null) return;
            for (int i = 0; i < flock.count; i++)
            {
                Gizmos.DrawWireCube(flock.boids[i].position, Vector3.one);
            }
        }

        void OnDestroy()
        {
            flock.Dispose();
        }
    }
}
