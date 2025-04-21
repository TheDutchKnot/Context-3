using UnityEngine;

public abstract class IndirestMeshSettings<T> : ScriptableObject
{
    [Header("Material & Shader"), SerializeField]
    string shaderBufferName = "_Positions";

    [field: SerializeField]
    public Material Material { get; private set; }
    public int ShaderBufferId { get; private set; }
    public RenderParams Params { get; private set; }

    protected virtual void OnEnable()
    {
        ShaderBufferId = Shader.PropertyToID(shaderBufferName);

        Params = new(Material)
        {
            worldBounds = new Bounds(Vector3.zero, Vector3.one * 100)
        };
    }

    public abstract T Create();
}