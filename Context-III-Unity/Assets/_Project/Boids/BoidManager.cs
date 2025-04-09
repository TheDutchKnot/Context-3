using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [SerializeField] BoidSettings settings;
    [SerializeField] Transform target;

    [SerializeField] int capacity = 500;
    [SerializeField] int amount = 4;

    Action<NativeArray<RaycastHit>, JobHandle> callback;
    QueryParameters queryParams;

    NativeArray<float3> velocities, hitNormals;
    NativeArray<Boid> boids;

    void Awake()
    {
        velocities = new NativeArray<float3>(capacity, Allocator.Persistent);
        hitNormals = new NativeArray<float3>(capacity, Allocator.Persistent);
        boids = new NativeArray<Boid>(capacity, Allocator.Persistent);

        queryParams = PhysXcastBatchProcessor.CreateQueryParameters(
            settings.collisionMask.value,
            settings.hitBackfaces,
            settings.hitTriggers,
            settings.hitMultiFace
            );

        callback = OnBoidCollisionCasts;

        for (int i = 0; i < capacity; i++)
        {
            boids[i] = new Boid
            {
                Position = transform.position + UnityEngine.Random.insideUnitSphere * 4,
                Rotation = transform.forward
            };
        }
    }

    void FixedUpdate()
    {
        PhysXcastBatchProcessor.PerformSpherecasts(boids, queryParams, settings.collisionRadius, settings.collisionRange, callback);
    }

    void OnBoidCollisionCasts(NativeArray<RaycastHit> hits, JobHandle handle)
    {
        var implicitHitNormalJob = new ImplicitHitNormalJob
        {
            normals = hitNormals,
            hits = hits
        };

        var implicitHitNormalJobHandle = implicitHitNormalJob.Schedule(amount, 1, handle);

        SteerBoids(); void SteerBoids()
        {
            if (amount < 1 || amount > capacity) return;

            var steerBoidsJob = new SteerBoids
            {
                velocities = velocities,
                hitNormals = hitNormals,
                boids = boids,

                perceptionRadius = settings.perceptionRadius,
                avoidanceRadius = settings.avoidanceRadius,

                seperationWeight = settings.seperationWeight,
                alignmentWeight = settings.alignmentWeight,
                collisionWeight = settings.collisionWeight,
                cohesionWeight = settings.cohesionWeight,
                targetWeight = settings.targetWeight,

                minSpeed = settings.minSpeed,
                maxSpeed = settings.maxSpeed,
                maxSteer = settings.maxSteer,

                deltaTime = Time.deltaTime,
                target = target.position
            };

            var steerBoidsHandle = steerBoidsJob.Schedule(amount, 1, implicitHitNormalJobHandle);

            var syncBoidsJob = new SyncBoids
            {
                Boids = boids,
                Vel = velocities,
                deltaTime = Time.deltaTime
            };

            var syncBoidsHandle = syncBoidsJob.Schedule(amount, 1, steerBoidsHandle);

            syncBoidsHandle.Complete();
        }
    }

    void OnDrawGizmos()
    {
        if (!boids.IsCreated) return;
        if (amount < 1 || amount > capacity) return;
        for (int i = 0; i < amount; i++)
        {
            Gizmos.DrawWireCube(boids[i].Position, Vector3.one);
        }
    }

    void OnDestroy()
    {
        if (boids.IsCreated)
            boids.Dispose();

        if (velocities.IsCreated)
            velocities.Dispose();
        
        if (hitNormals.IsCreated)
            hitNormals.Dispose();
    }
}