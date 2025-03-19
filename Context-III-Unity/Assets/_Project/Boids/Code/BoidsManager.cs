using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class BoidsManager : MonoBehaviour
{
    [SerializeField] Mesh mesh;
    [SerializeField] Material mat;
    [SerializeField] int count;

    GraphicsBuffer drawArgsBuf, dataBuf;

    NativeArray<float3> positions;

    RenderParams rp;

    void OnEnable()
    {
        var maxPosition = transform.position + transform.localScale / 2;
        var minPosition = transform.position - transform.localScale / 2;

        positions = MatrixFiller.Create(count, minPosition, maxPosition);

        drawArgsBuf = CreateDrawArgsBufferForRenderMeshIndirect(mesh, count);

        dataBuf = CreateDataBuffer<float3>(GraphicsBuffer.Target.Structured, count);

        dataBuf.SetData(positions);

        mat.SetBuffer("_Positions", dataBuf);

        rp = new RenderParams(mat)
        {
            receiveShadows = false,
            shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off,
            worldBounds = new Bounds(transform.position, transform.localScale)
        };
    }

    void Update()
    {
        Graphics.RenderMeshIndirect(rp, mesh, drawArgsBuf);
    }

    void OnDisable()
    {
        if (positions.IsCreated)
            positions.Dispose();

        drawArgsBuf.Release();
        drawArgsBuf = null;

        dataBuf.Release();
        dataBuf = null;
    }

    void OnValidate()
    {
        if (drawArgsBuf != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    static GraphicsBuffer CreateDrawArgsBufferForRenderMeshIndirect(Mesh mesh, int instanceCount)
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

    static GraphicsBuffer CreateDataBuffer<T>(GraphicsBuffer.Target target, int count) where T : struct
    {
        return new GraphicsBuffer(target, count, Marshal.SizeOf<T>());
    }
}
