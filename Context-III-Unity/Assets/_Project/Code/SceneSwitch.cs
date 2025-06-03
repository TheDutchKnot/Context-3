using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    public string sceneName; // Set in inspector

    private void OnTriggerEnter(Collider other)
    {
        // Adjust the tag if using XR Rig or controllers
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
