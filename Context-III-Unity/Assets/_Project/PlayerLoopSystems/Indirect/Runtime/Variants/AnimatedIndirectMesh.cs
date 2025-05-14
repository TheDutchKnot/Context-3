using Tdk.PlayerLoopSystems.Timers;
using Unity.Collections;
using UnityEngine;

namespace Tdk.PlayerLoopSystems.Indirect
{
    public class AnimatedIndirectMesh : IndirectMesh
    {
        readonly AnimatedIndirectMeshSettings settings;
        readonly FrequencyTimer timer;

        int animIndex;
        int dataLen;

        public AnimatedIndirectMesh(AnimatedIndirectMeshSettings settings)
        {
            this.settings = settings;

            timer = new(settings.Playbackrate)
            {
                OnTick = TickAnimation
            };
            timer.Start();

            argsBuf = CreateArgsBuffer(settings.Meshes[animIndex], dataLen);
        }

        public void SetData<T>(NativeArray<T> data) where T : struct
        {
            if (dataBuf == null || dataBuf.count != data.Length)
            {
                dataBuf?.Dispose();
                dataBuf = CreateDataBuffer<T>(data.Length);
                dataLen = data.Length;
            }

            NativeArray<T> bufferData = dataBuf.LockBufferForWrite<T>(0, data.Length);
            NativeArray<T>.Copy(data, bufferData);
            dataBuf.UnlockBufferAfterWrite<T>(data.Length);

            settings.Material.SetBuffer(settings.ShaderBufferId, dataBuf);
        }

        void TickAnimation()
        {
            animIndex++;
            if (animIndex >= settings.Meshes.Length)
            {
                animIndex = 0;
            }

            argsBuf?.Dispose();
            argsBuf = CreateArgsBuffer(settings.Meshes[animIndex], dataLen);
        }

        public override void RenderMeshIndirect()
        {
            Graphics.RenderMeshIndirect(settings.Params, settings.Meshes[animIndex], argsBuf);
        }

        public override void Dispose()
        {
            if (disposed) return;

            timer.Dispose();

            base.Dispose();
        }
    }
}