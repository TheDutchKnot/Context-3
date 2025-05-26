using UnityEngine;

namespace Tdk.PlayerLoopSystems.Indirect
{
    [CreateAssetMenu(menuName = "IndirectMeshSettings/Animated")]
    public class AnimatedIndirectMeshSettings : IndirestMeshSettings<AnimatedIndirectMesh>
    {
        [field: Header("Mesh Properties")]
        [field: SerializeField] public int Playbackrate { get; private set; }
        [field: SerializeField] public Mesh[] Meshes { get; private set; }
        [field: SerializeField] public Vector3 Scale { get; private set; } = Vector3.one;

        public override AnimatedIndirectMesh Create()
        {
            return new AnimatedIndirectMesh(this);
        }
    }
}