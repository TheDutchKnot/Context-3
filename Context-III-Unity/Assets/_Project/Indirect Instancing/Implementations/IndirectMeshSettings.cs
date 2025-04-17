using UnityEngine;

[CreateAssetMenu(menuName = "IndirectMeshSettings")]
public class IndirectMeshSettings : ScriptableObject
{
    [field: Header("Mesh Properties")]
    [field: SerializeField] public Mesh Mesh { get; private set; }

    [Header("Material Properties")]
    [SerializeField] string shaderBufferName = "_Positions";
    [field: SerializeField] public Material Material { get; private set; }


    [Header("Render Properties")]
    [SerializeField] Bounds bounds;

    public RenderParams RenderParams { get; private set; }
    public int BufferId { get; private set; }
    
    void OnEnable()
    {
        BufferId = Shader.PropertyToID(shaderBufferName);

        RenderParams = new(Material)
        {
            worldBounds = bounds
        };
    }

    public IndirectMesh Create()
    {
        return new IndirectMesh(this);
    }
}
