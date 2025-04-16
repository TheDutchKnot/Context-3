using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public static class IndirectMeshUtils
{
    public static GraphicsBuffer CreateDrawArgsBufferForRenderMeshIndirect(Mesh mesh, int instanceCount)
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

    public static GraphicsBuffer CreateDataBufferForRenderMeshIndirect<T>(int count)
    {
        return new GraphicsBuffer(GraphicsBuffer.Target.Structured, GraphicsBuffer.UsageFlags.LockBufferForWrite, count, UnsafeUtility.SizeOf(typeof(T)));
    }
}