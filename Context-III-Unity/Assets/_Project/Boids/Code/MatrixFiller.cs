using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

internal static class MatrixFiller
{
    public static NativeArray<float3> Create(int count, float3 minPosition, float3 maxPosition)
    {
        var transformMatrixArray = new NativeArray<float3>(count, Allocator.Persistent);
        var job = new InitializeMatrixJob
        {
            _transformMatrixArray = transformMatrixArray,
            _maxPosition = maxPosition,
            _minPosition = minPosition
        };

        var jobHandle = job.Schedule(count, 64);
        jobHandle.Complete();

        return transformMatrixArray;
    }

    [BurstCompile]
    struct InitializeMatrixJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<float3> _transformMatrixArray;
        [ReadOnly] public float3 _maxPosition;
        [ReadOnly] public float3 _minPosition;

        public void Execute(int index)
        {
            var random = new Unity.Mathematics.Random((uint)index + 1);
            var x = random.NextFloat(_minPosition.x, _maxPosition.x);
            var y = random.NextFloat(_minPosition.y, _maxPosition.y);
            var z = random.NextFloat(_minPosition.z, _maxPosition.z);

            _transformMatrixArray[index] = new float3(x, y, z);
        }
    }
}