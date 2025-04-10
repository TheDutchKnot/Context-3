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

    IncrementJob job;

    JobHandle handle;

    void Awake()
    {
        values = new NativeArray<float3>(load, Allocator.Persistent);

        job = new IncrementJob { A = values };

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
        handle = job.Schedule(load, 1);
        handle = job.Schedule(load, 1, handle);
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
        if (values.IsCreated)
            values.Dispose();

        EarlyUpdate.DeregisterEarlyUpdate(this);
        PreLateUpdate.DeregisterPreLateUpdate(this);
        PostLateUpdate.DeregisterPostLateUpdate(this);
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

public enum JobHandleStatus
{
    Running,
    AwaitingCompletion,
    Completed
}

public struct JobHandleExtended
{
    JobHandle handle;
    public JobHandleStatus status;

    public JobHandleExtended(JobHandle handle) : this()
    {
        this.handle = handle;
        //by default status is Running
    }

    public JobHandleStatus Status
    {
        get
        {
            if (status == JobHandleStatus.Running && handle.IsCompleted)
                status = JobHandleStatus.AwaitingCompletion;

            return status;
        }
    }

    public void Complete()
    {
        handle.Complete();
        status = JobHandleStatus.Completed;
    }

    public static implicit operator JobHandle(JobHandleExtended extended)
    {
        return extended.handle;
    }

    public static implicit operator JobHandleExtended(JobHandle handle)
    {
        return new JobHandleExtended(handle);
    }
}
