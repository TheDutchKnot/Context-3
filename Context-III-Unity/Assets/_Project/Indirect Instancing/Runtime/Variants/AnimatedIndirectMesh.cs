using Unity.Collections;
using UnityEngine;

public class AnimatedIndirectMesh : IndirectMesh
{
    readonly AnimatedIndirectMeshSettings settings;

    public AnimatedIndirectMesh(AnimatedIndirectMeshSettings settings)
    {
        this.settings = settings;
    }

    public void SetData<T>(NativeArray<T> data) where T : struct
    {
        if (dataBuf == null || dataBuf.count != data.Length)
        {
            dataBuf?.Dispose();
            dataBuf = CreateDataBuffer<T>(data.Length);
        }

        argsBuf?.Dispose();
        argsBuf = CreateArgsBuffer(settings.Mesh, data.Length);

        NativeArray<T> bufferData = dataBuf.LockBufferForWrite<T>(0, data.Length);
        NativeArray<T>.Copy(data, bufferData);
        dataBuf.UnlockBufferAfterWrite<T>(data.Length);

        settings.Material.SetBuffer(settings.ShaderBufferId, dataBuf);
    }

    public override void RenderMeshIndirect()
    {
        if (Time.time >= settings.LastTickTime + (1f / settings.AnimationFps))
        {
            settings.Mesh = settings.Meshes[settings.AnimationIndex];

            settings.AnimationIndex++;
            if (settings.AnimationIndex >= settings.Meshes.Length)
            {
                settings.AnimationIndex = 0;
            }
            settings.LastTickTime = Time.time;
        }
        settings.Tick++;

        Graphics.RenderMeshIndirect(settings.Params, settings.Mesh, argsBuf);
    }
}