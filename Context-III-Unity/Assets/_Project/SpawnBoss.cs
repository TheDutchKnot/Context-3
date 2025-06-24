using System.Collections;
using tdk.Boids;
using UnityEngine;
using UnityEngine.Playables;

public class SpawnBoss : MonoBehaviour
{
    [SerializeField] GameObject TimelineCutscene;
    [SerializeField] BoidManager boidsManager;

    [SerializeField] GameObject vfx;

    bool playing;

    public void Summon()
    {
        if (!playing)
        {
            StartCoroutine(SummonBoss());
            playing = true;
        }
    }

    IEnumerator SummonBoss()
    {
        boidsManager.SetOption(0);

        yield return new WaitForSeconds(3.5f);

        TimelineCutscene.SetActive(true);
        vfx.SetActive(true);

        yield return new WaitForSeconds((float)TimelineCutscene.GetComponent<PlayableDirector>().duration);

        boidsManager.SetOption(1);
        vfx.SetActive(false);
    }
}
