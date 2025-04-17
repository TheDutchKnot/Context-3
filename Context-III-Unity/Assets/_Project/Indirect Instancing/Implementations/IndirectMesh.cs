using Unity.Collections;
using UnityEngine;

public class IndirectMesh : IndirectMeshBase
{
    readonly IndirectMeshSettings settings;

    public IndirectMesh(IndirectMeshSettings settings) : base()
    {
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

    public override void RenderMeshIndirect()
    {
        Graphics.RenderMeshIndirect(settings.RenderParams, settings.Mesh, drawArgsBuf);
    }
}