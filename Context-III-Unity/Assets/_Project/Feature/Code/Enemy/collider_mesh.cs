using UnityEngine;

public class collider_mesh: MonoBehaviour
{
    public SkinnedMeshRenderer meshRenderer;
    public MeshCollider coll;
    void Update()
    {
        for (int i = 0; i < 5; i++)
        {
            Mesh colliderMesh = new Mesh();
            meshRenderer.BakeMesh(colliderMesh); //更新mesh
            coll.sharedMesh = null;
            coll.sharedMesh = colliderMesh; //将新的mesh赋给meshcollider
        }
    }

}
