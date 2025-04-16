using System;
using Tdk.PhysXcastBatchProcessor;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace tdk.Boids
{
    public class BoidManager : MonoBehaviour
    {
        [SerializeField] BoidSettings settings;
        [SerializeField] int capacity = 500;
        [SerializeField] Transform target;

        NativeArray<Boid> boids;
        NativeArray<float3> vel;

        NativeArray<SpherecastCommand> commands;
        NativeArray<RaycastHit> hitResults;

        void Awake()
        {
            boids = new NativeArray<Boid>(capacity, Allocator.Persistent);
            vel = new NativeArray<float3>(capacity, Allocator.Persistent);

            for (int i = 0; i < capacity; i++)
            {
                boids[i] = new Boid
                {
                    position = transform.position + UnityEngine.Random.insideUnitSphere * 4,
                    direction = transform.forward
                };
            }
        }

        void Update()
        {
            using (commands = new NativeArray<SpherecastCommand>(capacity, Allocator.TempJob))
            using (hitResults = new NativeArray<RaycastHit>(capacity, Allocator.TempJob))
            {
                var queryJobHandle = PhysXcastBatchProcessor.PerformSpherecasts(commands, hitResults, boids, settings.collisionMask.value);

                var steerJob = new SteerBoids
                {
                    boidVelocities = vel,
                    boids = boids,
                    hits = hitResults,

                    perceptionRadius = settings.perceptionRadius,
                    avoidanceRadius = settings.avoidanceRadius,

                    seperationWeight = settings.seperationWeight,
                    alignmentWeight = settings.alignmentWeight,
                    cohesionWeight = settings.cohesionWeight,

                    collisionWeight = settings.collisionWeight,
                    targetWeight = settings.targetWeight,

                    minSpeed = settings.minSpeed,
                    maxSpeed = settings.maxSpeed,
                    maxSteer = settings.maxSteer,

                    targetPosition = target.position,
                    deltaTime = Time.deltaTime
                };

                var steerJobHandle = steerJob.Schedule(capacity, 1, queryJobHandle);

                var syncJob = new SyncBoids
                {
                    Boids = boids,
                    Vel = vel,
                    deltaTime = Time.deltaTime
                };

                var syncJobHandle = syncJob.Schedule(capacity, 1, steerJobHandle);

                syncJobHandle.Complete();
            }
        }

        void OnDrawGizmos()
        {
            if (!boids.IsCreated) return;
            for (int i = 0; i < capacity; i++)
            {
                Gizmos.DrawWireCube(boids[i].position, Vector3.one);
            }
        }

        void OnDestroy()
        {
            if (boids.IsCreated) boids.Dispose();
            if (vel.IsCreated) vel.Dispose();
        }
    }
}
