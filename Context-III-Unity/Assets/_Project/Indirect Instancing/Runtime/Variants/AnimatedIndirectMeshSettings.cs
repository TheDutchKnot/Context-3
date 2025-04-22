using UnityEngine;

[CreateAssetMenu(menuName = "IndirectMeshSettings/Animated")]
public class AnimatedIndirectMeshSettings : IndirestMeshSettings<AnimatedIndirectMesh>
{
    [field: Header("Mesh Properties")]
    [field: SerializeField] public int AnimationFps { get; private set; }
    [field: SerializeField] public Mesh[] Meshes { get; private set; }

    public int AnimationIndex { get; set; }
    public int Tick { get; set; }
    public float LastTickTime { get; set; }
    public Mesh Mesh { get; set; }

    protected override void OnEnable()
    {
        base.OnEnable();

        Mesh = Meshes[0];
    }

    void OnDisable()
    {
        AnimationIndex = 0;
        Tick = 0;
        LastTickTime = 0;
        Mesh = null;
    }

    public override AnimatedIndirectMesh Create()
    {
        return new AnimatedIndirectMesh(this);
    }
}