using Unity.Collections;
using UnityEngine;

public class DynamicIndirectMesh : IndirectMesh
{
    readonly DynamicIndirectMeshSettings settings;

    public DynamicIndirectMesh(DynamicIndirectMeshSettings settings) : base()
    {
        this.settings = settings;
    }

    public void SetData<T>(NativeArray<T> data) where T : struct
    {
        if (dataBuf == null || dataBuf.count < data.Length)
        {
            dataBuf?.Dispose();
            dataBuf = CreateDataBuffer<T>(data.Length);

            argsBuf?.Dispose();
            argsBuf = CreateArgsBuffer(settings.Mesh, data.Length);
        }

        NativeArray<T> bufferData = dataBuf.LockBufferForWrite<T>(0, data.Length);
        NativeArray<T>.Copy(data, bufferData);
        dataBuf.UnlockBufferAfterWrite<T>(data.Length);

        settings.Material.SetBuffer(settings.ShaderBufferId, dataBuf);
    }

    public override void RenderMeshIndirect()
    {
        Graphics.RenderMeshIndirect(settings.Params, settings.Mesh, argsBuf);
    }
}