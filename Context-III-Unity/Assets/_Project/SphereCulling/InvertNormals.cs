using UnityEngine;

public class InvertNormals : MonoBehaviour
{
    public void Awake()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        mesh.normals = normals;

        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            (triangles[i + 2], triangles[i]) = (triangles[i], triangles[i + 2]);
        }
        mesh.triangles = triangles;
    }
}