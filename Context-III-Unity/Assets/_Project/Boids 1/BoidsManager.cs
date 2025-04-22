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
    int count;

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
        boids[count] = new()
        {
            position = position,
            direction = forward
        };
        count++;
    }

    NativeArray<SpherecastCommand> commands;
    NativeArray<RaycastHit> hitResults;

    public void UpdateBoids()
    {
        if (count == 0) return;

        using (commands = new NativeArray<SpherecastCommand>(count, Allocator.TempJob))
        using (hitResults = new NativeArray<RaycastHit>(count, Allocator.TempJob))
        {
            var queryJobHandle = PhysXcastBatchProcessor.PerformSpherecasts(
                commands, 
                hitResults, 
                boids.GetSubArray(0, count), 
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

            renderer.RenderMeshIndirect();
        }
    }

    void Remove(int i)
    {
        boids[i] = boids[count];
        count--;
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
