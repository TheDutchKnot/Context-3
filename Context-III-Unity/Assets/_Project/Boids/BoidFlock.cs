using System;
using Tdk.PhysXcastBatchProcessor;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace tdk.Boids
{
    public class BoidFlock : IDisposable
    {
        readonly BoidSettings settings;
        public Transform Target { private get; set; }

        GraphicsBuffer argsBuf, dataBuf;

        public NativeArray<Boid> boids;
        NativeArray<float3> vel;

        NativeArray<SpherecastCommand> commands;
        NativeArray<RaycastHit> hitResults;

        public int count = 0;

        public BoidFlock(BoidSettings settings)
        {
            this.settings = settings;

            InitArrays(); void InitArrays(){
                boids = new NativeArray<Boid>(settings.MaxCapacity, Allocator.Persistent);
                vel = new NativeArray<float3>(settings.MaxCapacity, Allocator.Persistent);

                for (int i = 0; i < settings.MaxCapacity; i++)
                {
                    boids[i] = new Boid
                    {
                        position = Vector3.zero,
                        direction = Vector3.forward
                    };
                }
            }
        }

        public void Add(Transform origin)
        {
            boids[count] = new Boid
            {
                direction = origin.forward,
                position = origin.position
            };
            count++;
        }

        public void UpdateBoids()
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

                    targetPosition = Target.position,
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

                SetData(boids.GetSubArray(0, count));

                Graphics.RenderMeshIndirect(settings.Params, settings.Mesh, argsBuf);
            }
        }

        public void SetData<T>(NativeArray<T> data) where T : struct
        {
            if (dataBuf == null || dataBuf.count < data.Length)
            {
                dataBuf?.Dispose();
                dataBuf = CreateDataBuffer<T>(data.Length);

                argsBuf?.Dispose();
                argsBuf = CreateArgsBuffer(settings.Mesh, data.Length);
            }

            NativeArray<T> bufferData = dataBuf.LockBufferForWrite<T>(0, data.Length);
            NativeArray<T>.Copy(data, bufferData);
            dataBuf.UnlockBufferAfterWrite<T>(data.Length);

            settings.Material.SetBuffer(settings.ShaderBufferId, dataBuf);
        }

        GraphicsBuffer CreateArgsBuffer(Mesh mesh, int instanceCount)
        {
            var commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
            commandData[0] = new GraphicsBuffer.IndirectDrawIndexedArgs
            {
                indexCountPerInstance = mesh.GetIndexCount(0),
                instanceCount = (uint)instanceCount,
                startIndex = mesh.GetIndexStart(0),
                baseVertexIndex = mesh.GetBaseVertex(0),
            };

            var drawArgsBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.IndirectArguments,
                1,
                GraphicsBuffer.IndirectDrawIndexedArgs.size
            );
            drawArgsBuffer.SetData(commandData);

            return drawArgsBuffer;
        }

        GraphicsBuffer CreateDataBuffer<T>(int dataCount)
        {
            return new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                GraphicsBuffer.UsageFlags.LockBufferForWrite,
                dataCount, UnsafeUtility.SizeOf(typeof(T)));
        }

        bool disposed;

        ~BoidFlock()
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
                if (boids.IsCreated) boids.Dispose();
                if (vel.IsCreated) vel.Dispose();

                argsBuf?.Dispose();
                dataBuf?.Dispose();
            }

            disposed = true;
        }
    }
}
