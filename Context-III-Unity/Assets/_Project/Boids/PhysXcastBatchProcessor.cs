using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;

namespace Tdk.PhysXcastBatchProcessor
{
    public interface IPhysXcast
    {
        public float3 Origin { get; }
        public float3 Direction { get; }
    }

    public static class PhysXcastBatchProcessor
    {
        const int innerLoopBatchCount = 1;

        public static JobHandle PerformRaycasts<T>(
            NativeArray<RaycastCommand> rayCommands,
            NativeArray<RaycastHit> hitResults,
            NativeArray<T> data,
            int layerMask,
            bool hitBackfaces = false,
            bool hitTriggers = false,
            bool hitMultiFace = false,
            float maxDistance = 1,
            JobHandle deps = default
            )
            where T : struct, IPhysXcast
        {
            var castParams = CreateQueryParameters(layerMask, hitBackfaces, hitTriggers, hitMultiFace);

            for (int i = 0; i < data.Length; i++)
            {
                rayCommands[i] = new RaycastCommand(data[i].Origin, data[i].Direction, castParams, maxDistance);
            }

            return RaycastCommand.ScheduleBatch(rayCommands, hitResults, innerLoopBatchCount, deps);
        }

        public static JobHandle PerformSpherecasts<T>(
            NativeArray<SpherecastCommand> sphereCommands,
            NativeArray<RaycastHit> hitResults,
            NativeArray<T> data,
            int layerMask,
            bool hitBackfaces = false,
            bool hitTriggers = false,
            bool hitMultiFace = false,
            float maxDistance = 1,
            float maxRadius = 1,
            JobHandle deps = default
            )
            where T : struct, IPhysXcast
        {
            var castParams = CreateQueryParameters(layerMask, hitBackfaces, hitTriggers, hitMultiFace);

            for (int i = 0; i < data.Length; i++)
            {
                sphereCommands[i] = new SpherecastCommand(data[i].Origin, maxRadius, data[i].Direction, castParams, maxDistance);
            }

            return SpherecastCommand.ScheduleBatch(sphereCommands, hitResults, innerLoopBatchCount, deps);
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
}