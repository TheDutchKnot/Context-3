using System;
using Tdk.PhysXcastBatchProcessor;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class BoidsManager : MonoBehaviour
{
    [SerializeField] DynamicIndirectMeshSettings renderingSettings;
    [SerializeField] BoidSettings swarmSettings;

    Boids swarm;

    void Awake()
    {
        swarm = new(swarmSettings, renderingSettings.Create());
        swarm.SetTarget(transform);
    }

    [ContextMenu("Add")]
    public void Add()
    {
        swarm.AddBoid(transform.position, transform.forward);
    }

    void Update()
    {
        swarm.UpdateBoids();
    }

    void OnDestroy()
    {
        swarm.Dispose();
    }
}

public class Boids : IDisposable
{
    readonly DynamicIndirectMesh renderer;
    readonly BoidSettings settings;

    NativeArray<Boid> boids;
    NativeArray<float3> vel;
    Transform target;
    int amount;

    public Boids(BoidSettings settings, DynamicIndirectMesh renderer)
    {
        this.renderer = renderer;
        this.settings = settings;

        InitBuffers(); void InitBuffers(){
            boids = new NativeArray<Boid>(settings.MaxCapacity, Allocator.Persistent);
            vel = new NativeArray<float3>(settings.MaxCapacity, Allocator.Persistent);
        }
    }

    public void SetTarget(Transform trans) => target = trans;

    public void AddBoid(Vector3 position, Vector3 forward)
    {
        boids[amount] = new()
        {
            position = position,
            direction = forward
        };
        amount++;
    }

    NativeArray<SpherecastCommand> commands;
    NativeArray<RaycastHit> hitResults;

    public void UpdateBoids()
    {
        if (amount == 0) return;

        using (commands = new NativeArray<SpherecastCommand>(amount, Allocator.TempJob))
        using (hitResults = new NativeArray<RaycastHit>(amount, Allocator.TempJob))
        {
            var queryJobHandle = PhysXcastBatchProcessor.PerformSpherecasts(
                commands, 
                hitResults, 
                boids.GetSubArray(0, amount), 
                settings.CollisionMask.value
                );

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

            var steerJobHandle = steerJob.Schedule(amount, 1, queryJobHandle);

            var syncJob = new SyncBoids
            {
                Boids = boids,
                Vel = vel,
                deltaTime = Time.deltaTime
            };

            var syncJobHandle = syncJob.Schedule(amount, 1, steerJobHandle);

            syncJobHandle.Complete();

            renderer.SetData(boids.GetSubArray(0, amount));

            renderer.RenderMeshIndirect();
        }
    }

    bool disposed;

    ~Boids()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            if (boids.IsCreated) 
                boids.Dispose();

            if (vel.IsCreated) 
                vel.Dispose();

            renderer.Dispose();
        }

        disposed = true;
    }
}
