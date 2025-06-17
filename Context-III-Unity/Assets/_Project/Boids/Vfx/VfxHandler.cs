using UnityEngine;

public class VfxHandler : MonoBehaviour
{
    [SerializeField] ParticleSystem particleVortex;
    [SerializeField] ParticleSystem particleEye;

    public void Play(Transform location)
    {
        transform.position = location.position;

        particleVortex.Play();
        particleEye.Play();

        Invoke(nameof(Stop), 3f);
    }

    public void Stop()
    {
        particleVortex.Stop();
        particleEye.Stop();
    }
}
