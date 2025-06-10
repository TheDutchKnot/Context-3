using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    public GameObject fadeOut;

    private void OnTriggerEnter(Collider other)
    {
        // Adjust the tag if using XR Rig or controllers
        if (other.CompareTag("Player"))
        {
            fadeOut.SetActive(true);
        }
    }
}
