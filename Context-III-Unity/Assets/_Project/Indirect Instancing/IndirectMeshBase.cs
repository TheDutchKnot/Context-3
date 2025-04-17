using UnityEngine;
using System;

public abstract class IndirectMeshBase : IRenderMeshIndirect, IDisposable
{
    protected GraphicsBuffer drawArgsBuf, dataBuf;

    public IndirectMeshBase()
    {
        IndirectMeshManager.RegisterInstance(this);
    }

    public abstract void RenderMeshIndirect();

    bool disposed;

    ~IndirectMeshBase()
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

            IndirectMeshManager.DeregisterInstance(this);
        }

        disposed = true;
    }
}
