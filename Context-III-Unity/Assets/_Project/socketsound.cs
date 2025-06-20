using UnityEngine;

public class socketsound : MonoBehaviour
{
    [SerializeField] AudioSource source;

    public void PlaySound()
    {
        source.Play();
    }
}
