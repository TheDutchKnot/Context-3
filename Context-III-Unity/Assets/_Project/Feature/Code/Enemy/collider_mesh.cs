using UnityEngine;

public class collider_mesh: MonoBehaviour
{    SkinnedMeshRenderer _smr;
    MeshCollider        _mc;
    MeshFilter          _mf;

    Mesh _fullBake;
    Mesh _subBake;

    void Awake()
    {
        _smr = GetComponent<SkinnedMeshRenderer>();
        _mc  = GetComponent<MeshCollider>();
        _mf  = GetComponent<MeshFilter>();

        // Collider: non-convex + kinematic Rigidbody
        _mc.convex = false;
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        // prepare two buffers
        _fullBake = new Mesh { name = _smr.sharedMesh.name + "_FullBake" };
        _subBake  = new Mesh { name = _smr.sharedMesh.name + "_SubBake" };

        // start collider on the empty subBake
        _mc.sharedMesh = _subBake;

        // start filter on the empty subBake
        // use mesh (instance) so you don’t overwrite the asset
        _mf.mesh = _subBake;
    }

    void LateUpdate()
    {
        // 1) Bake the entire animated mesh
        _smr.BakeMesh(_fullBake);

        // 2) Rebuild only the one sub-mesh (index 0)
        _subBake.Clear();
        _subBake.vertices = _fullBake.vertices;
        _subBake.normals  = _fullBake.normals;
        _subBake.uv       = _fullBake.uv;
        // Copy uv2/colors here if needed
        _subBake.triangles = _fullBake.GetTriangles(0);
        _subBake.RecalculateBounds();

        // 3a) Force the collider to re-cook
        _mc.sharedMesh = null;
        _mc.sharedMesh = _subBake;

        // 3b) Update the MeshFilter to show the same shape
        _mf.mesh = _subBake;

        Physics.SyncTransforms();
    }
}
