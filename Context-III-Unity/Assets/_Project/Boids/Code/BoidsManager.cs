using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class BoidsManager : MonoBehaviour
{
    [SerializeField] IndirectMeshSettings settings;
    [SerializeField] int count;

    IndirectMesh indirectMesh;

    NativeArray<float3> positions;

    void OnValidate()
    {
        if (positions.IsCreated && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    void Awake()
    {
        indirectMesh = settings.Create();
    }

    void OnEnable()
    {
        var maxPosition = transform.position + transform.localScale / 2;
        var minPosition = transform.position - transform.localScale / 2;

        positions = MatrixFiller.Create(count, minPosition, maxPosition);

        indirectMesh.SetData(positions);
    }

    void Update()
    {
        IndirectMeshRenderer.RenderInstancedIndirect();
    }

    void OnDestroy()
    {
        indirectMesh?.Dispose();
    }

    void OnDisable()
    {
        if (positions.IsCreated)
            positions.Dispose();
    }
}
