using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using System;

public class PhysXcastBatchProcessor : MonoBehaviour
{
    public interface IPhysXcast
    {
        public float3 Origin { get; }
        public float3 Vector { get; }
    }

    const int maxCastsPerJob = 10000;
    const int maxHitsPerCast = 1;
    const int loopBatchCount = 1;

    static NativeArray<SpherecastCommand> sphereCommands;
    static NativeArray<RaycastCommand> rayCommands;
    static NativeArray<RaycastHit> hitResults;

    public static void PerformSpherecasts<T>(NativeArray<T> casts, QueryParameters castParams, float radius, float maxDistance, Action<NativeArray<RaycastHit>, JobHandle> callback) where T : struct, IPhysXcast
    {
        int castCount = Mathf.Min(casts.Length, maxCastsPerJob);
        int totalHits = castCount * maxHitsPerCast;

        using (sphereCommands = new NativeArray<SpherecastCommand>(castCount, Allocator.TempJob))
        {
            var setupSphereCommandJob = new SetupSpherecastCommandsJob<T>
            {
                castCommands = sphereCommands,
                maxDistance = maxDistance,
                castParams = castParams,
                radius = radius,
                casts = casts
            };

            var setupCommandJobHandle = setupSphereCommandJob.Schedule(castCount, loopBatchCount);

            using (hitResults = new NativeArray<RaycastHit>(totalHits, Allocator.TempJob))
            {
                var spherecastJobHandle = SpherecastCommand.ScheduleBatch(sphereCommands, hitResults, loopBatchCount, setupCommandJobHandle);

                callback?.Invoke(hitResults, spherecastJobHandle);
            }
        }
    }

    struct SetupSpherecastCommandsJob<T> : IJobParallelFor where T : struct, IPhysXcast
    {
        [WriteOnly] public NativeArray<SpherecastCommand> castCommands;
        [ReadOnly] public QueryParameters castParams;
        [ReadOnly] public NativeArray<T> casts;
        [ReadOnly] public float maxDistance;
        [ReadOnly] public float radius;

        public void Execute(int i)
        {
            castCommands[i] = new SpherecastCommand(casts[i].Origin, radius, casts[i].Vector, castParams, maxDistance);
        }
    }

    public static void PerformRaycasts<T>(NativeArray<T> casts, QueryParameters castParams, float maxDistance, Action<NativeArray<RaycastHit>, JobHandle> callback) where T : struct, IPhysXcast
    {
        int castCount = Mathf.Min(casts.Length, maxCastsPerJob);
        int totalHits = castCount * maxHitsPerCast;

        using (rayCommands = new NativeArray<RaycastCommand>(castCount, Allocator.TempJob))
        {
            var setupRayCommandJob = new SetupRaycastCommandsJob<T>
            {
                castCommands = rayCommands,
                maxDistance = maxDistance,
                castParams = castParams,
                casts = casts
            };

            var setupRayCommandJobHandle = setupRayCommandJob.Schedule(castCount, loopBatchCount);

            using (hitResults = new NativeArray<RaycastHit>(totalHits, Allocator.TempJob))
            {
                var raycastJobHandle = RaycastCommand.ScheduleBatch(rayCommands, hitResults, loopBatchCount, setupRayCommandJobHandle);

                callback?.Invoke(hitResults, raycastJobHandle);
            }
        }
    }

    struct SetupRaycastCommandsJob<T> : IJobParallelFor where T : struct, IPhysXcast
    {
        [WriteOnly] public NativeArray<RaycastCommand> castCommands;
        [ReadOnly] public QueryParameters castParams;
        [ReadOnly] public NativeArray<T> casts;
        [ReadOnly] public float maxDistance;

        public void Execute(int i)
        {
            castCommands[i] = new RaycastCommand(casts[i].Origin, casts[i].Vector, castParams, maxDistance);
        }
    }

    public static QueryParameters CreateQueryParameters(int layermask, bool hitBackfaces, bool hitTriggers, bool hitMultiFace)
    {
        QueryTriggerInteraction queryTriggerInteraction = hitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        return new QueryParameters
        {
            hitTriggers = queryTriggerInteraction,
            hitMultipleFaces = hitMultiFace,
            hitBackfaces = hitBackfaces,
            layerMask = layermask
        };
    }
}