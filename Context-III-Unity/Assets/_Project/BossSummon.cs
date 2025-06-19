using System.Collections;
using tdk.Boids;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

public class BossSummon : XRInteractableAffordanceStateProvider
{
    [SerializeField] BoidManager boidManager;
    [SerializeField] int boidBurstAmount = 3;
    [SerializeField] float interval = 5;
    [SerializeField] float duration = 10;

    [SerializeField] SpawnBoss boss;
    [SerializeField] GameObject vfx;

    Animator anim;

    static bool wasOpened;
    bool flag;
    bool done;

    new void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SummonBoids()
    {
        wasOpened = true;

        anim.SetTrigger("TrOpenBook");
        anim.SetTrigger("TrOpenPages");

        Invoke(nameof(PlayPortalAnim), 1f);

        gameObject.layer = LayerMask.NameToLayer("CaptureBoid");
    }

    public void CloseBook()
    {
        if (done)
        {
            anim.SetTrigger("TrCloseBook");
            anim.SetTrigger("TrClosePages");

            Invoke(nameof(StopPortalAnim), 1f);

            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    new void Update()
    {
        if (!wasOpened) return;

        if (!flag)
        {
            InvokeRepeating(nameof(IntervalSummon), 1f, interval);
            StartCoroutine(StopBurst(duration));
            flag = true;

            anim.SetTrigger("TrOpenBook");
            anim.SetTrigger("TrOpenPages");

            Invoke(nameof(PlayPortalAnim), 1f);
        }
    }

    void IntervalSummon()
    {
        for (int i = 0; i < boidBurstAmount; i++)
        {
            boidManager.Add(transform);
        }
    }

    void PlayPortalAnim() => vfx.SetActive(true);
    void StopPortalAnim() => vfx.SetActive(false);

    IEnumerator StopBurst(float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
        }
        CancelInvoke(nameof(IntervalSummon));

        done = true;
        CloseBook();

        Invoke(nameof(StopPortalAnim), 1f);

        boss.Summon();
    }
}
