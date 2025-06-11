using UnityEngine;
using UnityEngine.XR;

public class OcclusionMeshBorder : MonoBehaviour
{
    void Start()
    {
        XRSettings.useOcclusionMesh = false;
    }
}
