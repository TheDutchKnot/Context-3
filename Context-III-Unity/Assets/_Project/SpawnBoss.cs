using System.Collections;
using tdk.Boids;
using UnityEngine;
using UnityEngine.Playables;

public class SpawnBoss : MonoBehaviour
{
    [SerializeField] GameObject TimelineCutscene;
    [SerializeField] BoidManager boidsManager;

    [SerializeField] GameObject Portal;

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
        Portal.SetActive(true);

        yield return new WaitForSeconds((float)TimelineCutscene.GetComponent<PlayableDirector>().duration);
        Portal.SetActive(false);

        boidsManager.SetOption(1);
    }
}
