using Tdk.PhysXcastBatchProcessor;
using Tdk.PlayerLoopSystems.Indirect;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace tdk.Boids
{
    public class BoidManager : MonoBehaviour
    {
        [SerializeField] AnimatedIndirectMeshSettings rendererSettings;
        [SerializeField] BoidSettings settings;
        [SerializeField] int count = 0;
        [SerializeField] Transform target;

        NativeArray<Boid> boids;
        NativeArray<float3> vel;

        NativeArray<SpherecastCommand> commands;
        NativeArray<RaycastHit> hitResults;

        AnimatedIndirectMesh renderer;

        void Awake()
        {
            boids = new NativeArray<Boid>(settings.MaxCapacity, Allocator.Persistent);
            vel = new NativeArray<float3>(settings.MaxCapacity, Allocator.Persistent);

            for (int i = 0; i < settings.MaxCapacity; i++)
            {
                boids[i] = new Boid
                {
                    position = transform.position + UnityEngine.Random.insideUnitSphere * 4,
                    direction = transform.forward
                };
            }

            renderer = rendererSettings.Create();

            if (count > settings.MaxCapacity)
            {
                InvokeRepeating(nameof(Add), 4, 4);
            }
        }

        public void Add()
        {
            boids[count] = new()
            {
                position = transform.position,
                direction = transform.forward
            };
            count++;
        }

        void Update()
        {
            if (count == 0) return;

            using (commands = new NativeArray<SpherecastCommand>(count, Allocator.TempJob))
            using (hitResults = new NativeArray<RaycastHit>(count, Allocator.TempJob))
            {
                var queryJobHandle = PhysXcastBatchProcessor.PerformSpherecasts(commands, hitResults, boids, settings.CollisionMask.value);

                var steerJob = new SteerBoids
                {
                    boidVelocities = vel,
                    boids = boids,
                    hits = hitResults,

                    perceptionRadius = settings.PerceptionRadius,
                    avoidanceRadius = settings.AvoidanceRadius,

                    seperationWeight = settings.SeperationWeight,
                    alignmentWeight = settings.AlignmentWeight,
                    cohesionWeight = settings.CohesionWeight,

                    collisionWeight = settings.CollisionWeight,
                    targetWeight = settings.TargetWeight,

                    minSpeed = settings.MinSpeed,
                    maxSpeed = settings.MaxSpeed,
                    maxSteer = settings.MaxSteer,

                    targetPosition = target.position,
                    deltaTime = Time.deltaTime
                };

                var steerJobHandle = steerJob.Schedule(count, 1, queryJobHandle);

                var syncJob = new SyncBoids
                {
                    Boids = boids,
                    Vel = vel,
                    deltaTime = Time.deltaTime
                };

                var syncJobHandle = syncJob.Schedule(count, 1, steerJobHandle);

                syncJobHandle.Complete();

                renderer.SetData(boids.GetSubArray(0, count));
            }
        }

        void OnDestroy()
        {
            if (boids.IsCreated) boids.Dispose();
            if (vel.IsCreated) vel.Dispose();
            renderer.Dispose();
        }
    }
}
