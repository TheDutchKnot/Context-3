using UnityEngine;

public class VfxHandler : MonoBehaviour
{
    [SerializeField] GameObject vfxObject;

    public void Play()
    {
        vfxObject.SetActive(true);
    }

    public void Stop()
    {
        vfxObject.SetActive(false);
    }
}
