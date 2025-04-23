using System.Runtime.CompilerServices;
using Tdk.PhysXcastBatchProcessor;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct Boid : IPhysXcast
{
    public float3 position;
    public float3 direction;

    float3 IPhysXcast.Origin => position;
    float3 IPhysXcast.Direction => direction;
}

public static class Float3Ext
{
    public static readonly float3 up = new(0, 1, 0);

    public static readonly float3 forward = new(1, 0, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SqrMagnitude(float3 vector)
    {
        return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ClampMagnitude(float3 vector, float maxLength)
    {
        float num = SqrMagnitude(vector);
        if (num > maxLength * maxLength)
        {
            float num2 = math.sqrt(num);
            float num3 = vector.x / num2;
            float num4 = vector.y / num2;
            float num5 = vector.z / num2;
            return new float3(num3 * maxLength, num4 * maxLength, num5 * maxLength);
        }

        return vector;
    }
}

[BurstCompile]
public struct SteerBoids : IJobParallelFor
{
    [ReadOnly] public NativeArray<RaycastHit> hits;
    [ReadOnly] public NativeArray<Boid> boids;
    public NativeArray<float3> boidVelocities;

    [ReadOnly] public float perceptionRadius;
    [ReadOnly] public float avoidanceRadius;

    [ReadOnly] public float3 targetPosition;

    [ReadOnly] public float seperationWeight;
    [ReadOnly] public float alignmentWeight;
    [ReadOnly] public float cohesionWeight;

    [ReadOnly] public float collisionWeight;
    [ReadOnly] public float targetWeight;

    [ReadOnly] public float minSpeed;
    [ReadOnly] public float maxSpeed;
    [ReadOnly] public float maxSteer;

    [ReadOnly] public float deltaTime;

    public void Execute(int indexA)
    {
        float3 seperation = float3.zero;
        float3 alignment = float3.zero;
        float3 cohesion = float3.zero;
        int numFlockmates = 0;

        var boidAPosition = boids[indexA].position;

        for (int indexB = 0; indexB < boids.Length; indexB++)
        {
            if (indexA != indexB)
            {
                Boid boidB = boids[indexB];
                var offset = boidB.position - boidAPosition;
                var sqrDst = Float3Ext.SqrMagnitude(offset);

                if (sqrDst < math.square(perceptionRadius))
                {
                    numFlockmates += 1;
                    alignment += boidB.direction;
                    cohesion += boidB.position;

                    if (sqrDst < math.square(avoidanceRadius))
                    {
                        seperation -= offset / sqrDst;
                    }
                }
            }
        }

        var velocity = boidVelocities[indexA];
        
        var acceleration = 
            SteerTowards(targetPosition - boidAPosition, velocity) * targetWeight;

        acceleration += SteerTowards(hits[indexA].normal, velocity) * collisionWeight;

        if (numFlockmates != 0)
        {
            cohesion /= numFlockmates;
            cohesion -= boidAPosition;

            acceleration +=
                SteerTowards(seperation, velocity) * seperationWeight +
                SteerTowards(alignment, velocity) * alignmentWeight +
                SteerTowards(cohesion, velocity) * cohesionWeight;
        }

        velocity += acceleration * deltaTime;
        var speed = math.length(velocity);
        var dir = velocity / speed;

        speed = math.clamp(speed, minSpeed, maxSpeed);
        boidVelocities[indexA] = dir * speed;
    }

    readonly float3 SteerTowards(float3 vector, float3 velocity)
    {
        var v = math.normalizesafe(vector, float3.zero) * maxSpeed - velocity;
        return Float3Ext.ClampMagnitude(v, maxSteer);
    }
}

[BurstCompile(FloatPrecision = FloatPrecision.Medium, FloatMode = FloatMode.Fast)]
public struct SyncBoids : IJobParallelFor
{
    public NativeArray<Boid> Boids;
    [ReadOnly]
    public NativeArray<float3> Vel;

    [ReadOnly] public float deltaTime;

    public void Execute(int i)
    {
        Boids[i] = new Boid { position = Boids[i].position + (Vel[i] * deltaTime), direction = Vel[i] };
    }
}