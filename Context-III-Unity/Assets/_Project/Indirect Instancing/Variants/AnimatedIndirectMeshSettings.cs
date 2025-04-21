using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "IndirectMeshSettings/Animated")]
public class AnimatedIndirectMeshSettings : IndirestMeshSettings<AnimatedIndirectMesh>
{
    [field: Header("Mesh Properties")]
    [field: SerializeField]
    public Mesh[] Meshes { get; private set; }

    public override AnimatedIndirectMesh Create()
    {
        return new AnimatedIndirectMesh(this);
    }
}