using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System;

namespace Tdk.PlayerLoopSystems.Indirect
{
    public abstract class IndirectMesh : IRenderMeshIndirect, IDisposable
    {
        protected GraphicsBuffer argsBuf, dataBuf;

        public IndirectMesh()
        {
            IndirectMeshManager.RegisterInstance(this);
        }

        public abstract void RenderMeshIndirect();

        protected GraphicsBuffer CreateArgsBuffer(Mesh mesh, int instanceCount)
        {
            var commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
            commandData[0] = new GraphicsBuffer.IndirectDrawIndexedArgs
            {
                indexCountPerInstance = mesh.GetIndexCount(0),
                instanceCount = (uint)instanceCount,
                startIndex = mesh.GetIndexStart(0),
                baseVertexIndex = mesh.GetBaseVertex(0),
            };

            var drawArgsBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.IndirectArguments,
                1,
                GraphicsBuffer.IndirectDrawIndexedArgs.size
            );
            drawArgsBuffer.SetData(commandData);

            return drawArgsBuffer;
        }

        protected GraphicsBuffer CreateDataBuffer<T>(int dataCount)
        {
            return new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                GraphicsBuffer.UsageFlags.LockBufferForWrite,
                dataCount, UnsafeUtility.SizeOf(typeof(T)));
        }

        protected bool disposed;

        ~IndirectMesh()
        {
            Dispose(false);
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                argsBuf?.Dispose();
                dataBuf?.Dispose();

                IndirectMeshManager.DeregisterInstance(this);
            }

            disposed = true;
        }
    }
}