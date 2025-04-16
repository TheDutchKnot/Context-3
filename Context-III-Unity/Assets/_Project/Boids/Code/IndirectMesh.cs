using Unity.Collections;
using UnityEngine;
using System;

public class IndirectMesh : IRenderMeshIndirect, IDisposable
{
    readonly IndirectMeshSettings settings;

    GraphicsBuffer drawArgsBuf, dataBuf;

    public IndirectMesh(IndirectMeshSettings settings)
    {
        IndirectMeshRenderer.RegisterInstance(this);

        this.settings = settings;
    }

    public void SetData<T>(NativeArray<T> data) where T : struct
    {
        if (dataBuf == null || dataBuf.count < data.Length)
        {
            dataBuf?.Dispose();
            dataBuf = IndirectMeshUtils.CreateDataBufferForRenderMeshIndirect<T>(data.Length);

            drawArgsBuf?.Dispose();
            drawArgsBuf = IndirectMeshUtils.CreateDrawArgsBufferForRenderMeshIndirect(settings.Mesh, data.Length);
        }

        NativeArray<T> bufferData = dataBuf.LockBufferForWrite<T>(0, data.Length);
        NativeArray<T>.Copy(data, bufferData);
        dataBuf.UnlockBufferAfterWrite<T>(data.Length);

        settings.Material.SetBuffer(settings.BufferId, dataBuf);
    }

    void IRenderMeshIndirect.RenderMeshIndirect()
    {
        Graphics.RenderMeshIndirect(settings.RenderParams, settings.Mesh, drawArgsBuf);
    }

    bool disposed;

    ~IndirectMesh()
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
            drawArgsBuf?.Dispose();
            dataBuf?.Dispose();

            IndirectMeshRenderer.DeregisterInstance(this);
        }

        disposed = true;
    }
}