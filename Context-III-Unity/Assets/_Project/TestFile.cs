using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using Tdk.PlayerLoopBootstrapper.Samples;

public class TestFile : MonoBehaviour, IEarlyUpdate, IPreLateUpdate, IPostLateUpdate
{
    NativeArray<float3> values;

    [SerializeField]
    int load = 10000;

    JobHandle handle;

    void Awake()
    {
        values = new NativeArray<float3>(load, Allocator.Persistent);

        EarlyUpdate.RegisterEarlyUpdate(this);
        PreLateUpdate.RegisterPreLateUpdate(this);
        PostLateUpdate.RegisterPostLateUpdate(this);
    }

    void Update()
    {
        for (int i = 0; i < load * 10; i++)
        {
            var laod = Vector3.one * 2;
        }
    }

    public void OnEarlyUpdate()
    {
        var jobA = new IncrementJob { A = values };
        
        handle = jobA.Schedule(load, 1, handle);

        var jobB = new IncrementJob { A = values };

        handle = jobB.Schedule(load, 1, handle);

        JobHandle.ScheduleBatchedJobs();
    }

    public void OnPreLateUpdate()
    {
        if (handle.IsCompleted)
        {
            handle.Complete();
            MainThread();
        }
    }

    public void OnPostLateUpdate()
    {
        if (this == null) return;
        if (!handle.IsCompleted)
        {
            handle.Complete();
            MainThread();
        }
    }

    void MainThread()
    {
        
    }

    void OnDestroy()
    {
        EarlyUpdate.DeregisterEarlyUpdate(this);
        PreLateUpdate.DeregisterPreLateUpdate(this);
        PostLateUpdate.DeregisterPostLateUpdate(this);

        if (values.IsCreated)
            values.Dispose();
    }
}

public struct IncrementJob : IJobParallelFor
{
    public NativeArray<float3> A;

    public void Execute(int index)
    {
        A[index] = A[index]++;
    }
}