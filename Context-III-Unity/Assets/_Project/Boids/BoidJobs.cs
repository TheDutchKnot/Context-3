using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct Boid : PhysXcastBatchProcessor.IPhysXcast
{
    public float3 Position;
    public float3 Rotation;

    public float3 Origin => Position;
    public float3 Vector => Rotation;
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
            float num2 = (float)math.sqrt(num);
            float num3 = vector.x / num2;
            float num4 = vector.y / num2;
            float num5 = vector.z / num2;
            return new float3(num3 * maxLength, num4 * maxLength, num5 * maxLength);
        }

        return vector;
    }
}

[BurstCompile(FloatPrecision = FloatPrecision.Low)]
public struct ImplicitHitNormalJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<RaycastHit> hits;

    [WriteOnly]
    public NativeArray<float3> normals;

    public void Execute(int i)
    {
        normals[i] = hits[i].normal;
    }
}

[BurstCompile(FloatPrecision = FloatPrecision.Medium, FloatMode = FloatMode.Fast)]
public struct SteerBoids : IJobParallelFor
{
    public NativeArray<float3> velocities;
    [ReadOnly]
    public NativeArray<Boid> boids;

    [ReadOnly]
    public NativeArray<float3> hitNormals;

    [ReadOnly] public float perceptionRadius;
    [ReadOnly] public float avoidanceRadius;

    [ReadOnly] public float seperationWeight;
    [ReadOnly] public float alignmentWeight;
    [ReadOnly] public float cohesionWeight;

    [ReadOnly] public float collisionWeight;
    [ReadOnly] public float targetWeight;

    [ReadOnly] public float minSpeed;
    [ReadOnly] public float maxSpeed;
    [ReadOnly] public float maxSteer;

    [ReadOnly] public float deltaTime;

    [ReadOnly] public float3 target;

    public void Execute(int indexA)
    {
        var flockAvoidance = float3.zero;
        var flockHeading = float3.zero;
        var flockCentre = float3.zero;
        int numFlockmates = 0;

        float3 boidAPosition = boids[indexA].Position;

        for (int indexB = 0; indexB < boids.Length; indexB++)
        {
            if (indexA != indexB)
            {
                Boid boidB = boids[indexB];
                var offset = boidB.Position - boidAPosition;
                var sqrDst = Float3Ext.SqrMagnitude(offset);

                if (sqrDst < math.square(perceptionRadius))
                {
                    numFlockmates += 1;
                    flockHeading += boidB.Rotation;
                    flockCentre += boidB.Position;

                    if (sqrDst < math.square(avoidanceRadius))
                    {
                        flockAvoidance -= offset / sqrDst;
                    }
                }
            }
        }

        var acceleration = float3.zero;
        var velocity = velocities[indexA];

        float3 offsetToTarget = target - boidAPosition;
        acceleration += SteerTowards(offsetToTarget, velocity) * targetWeight;

        if (numFlockmates != 0)
        {
            float3 centreFlockMates = flockCentre / numFlockmates;

            float3 offsetFlockMatesCentre = centreFlockMates - boidAPosition;

            acceleration += SteerTowards(offsetFlockMatesCentre, velocity) * cohesionWeight;
            acceleration += SteerTowards(flockAvoidance, velocity) * seperationWeight;
            acceleration += SteerTowards(flockHeading, velocity) * alignmentWeight;
        }

        if (!hitNormals[indexA].Equals(float3.zero))
        {
            acceleration += SteerTowards(hitNormals[indexA], velocity) * collisionWeight;
        }

        velocity += acceleration * deltaTime;
        var speed = math.length(velocity);
        var dir = velocity / speed;
        speed = math.clamp(speed, minSpeed, maxSpeed);
        velocities[indexA] = dir * speed;
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
        Boids[i] = new Boid { Position = Boids[i].Position + (Vel[i] * deltaTime), Rotation = Vel[i] };
    }
}