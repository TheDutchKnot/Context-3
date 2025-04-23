using UnityEngine;

namespace Tdk.PlayerLoopSystems.Indirect
{
    [CreateAssetMenu(menuName = "IndirectMeshSettings/Dynamic")]
    public class DynamicIndirectMeshSettings : IndirestMeshSettings<DynamicIndirectMesh>
    {
        [field: Header("Mesh Properties")]
        [field: SerializeField] public Mesh Mesh { get; private set; }

        public override DynamicIndirectMesh Create()
        {
            return new DynamicIndirectMesh(this);
        }
    }
}