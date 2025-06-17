using Tdk.PlayerLoopSystems.Timers;
using Unity.Collections;
using UnityEngine;

namespace Tdk.PlayerLoopSystems.Indirect
{
    public class AnimatedIndirectMesh : IndirectMesh
    {
        readonly AnimatedIndirectMeshSettings settings;
        readonly FrequencyTimer timer;

        NativeArray<Matrix4x4> boidTRS;

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

        public void SetData(NativeArray<Matrix4x4> data)
        {
            if (boidTRS == null || boidTRS.Length != data.Length)
            {
                if (data.Length == 0)
                {
                    dataLen = 0;
                    return;
                }

                if (!boidTRS.IsCreated)
                    boidTRS.Dispose();

                boidTRS = new NativeArray<Matrix4x4>(data.Length, Allocator.Persistent);
                dataLen = data.Length;
            }

            if (!boidTRS.IsCreated) return;

            data.CopyTo(boidTRS);
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
            if (dataLen == 0) return;
            //Graphics.RenderMeshIndirect(settings.Params, settings.Meshes[animIndex], argsBuf);
            Graphics.RenderMeshInstanced(settings.Params, settings.Meshes[animIndex], 0, boidTRS);
        }

        public void RenderInstancedManual(NativeArray<Matrix4x4> data)
        {
            if (data.Length == 0) 
                return;

            Graphics.RenderMeshInstanced(settings.Params, settings.Meshes[animIndex], 0, data);
        }

        public override void Dispose()
        {
            if (disposed) return;

            if (boidTRS.IsCreated)
                boidTRS.Dispose();

            timer.Dispose();

            base.Dispose();
        }
    }
}