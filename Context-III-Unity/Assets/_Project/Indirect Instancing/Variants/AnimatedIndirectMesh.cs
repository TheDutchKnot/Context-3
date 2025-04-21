using Unity.Collections;

public class AnimatedIndirectMesh : IndirectMesh
{
    readonly AnimatedIndirectMeshSettings settings;

    public AnimatedIndirectMesh(AnimatedIndirectMeshSettings settings)
    {
        this.settings = settings;
    }

    public void SetData<T>(NativeArray<T> data) where T : struct
    {
        if (dataBuf == null || dataBuf.count < data.Length)
        {
            dataBuf?.Dispose();
            dataBuf = CreateDataBuffer<T>(data.Length);
        }

        NativeArray<T> bufferData = dataBuf.LockBufferForWrite<T>(0, data.Length);
        NativeArray<T>.Copy(data, bufferData);
        dataBuf.UnlockBufferAfterWrite<T>(data.Length);

        settings.Material.SetBuffer(settings.ShaderBufferId, dataBuf);
    }

    public override void RenderMeshIndirect()
    {
        throw new System.NotImplementedException();
    }
}