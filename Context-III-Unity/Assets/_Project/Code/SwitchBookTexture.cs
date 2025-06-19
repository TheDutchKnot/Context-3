using tdk.Boids;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;


public class SwitchBookTexture : XRInteractableAffordanceStateProvider
{
    [Header("Things to fill in")]
    [SerializeField] Material highlitedMaterial;
    [SerializeField] Material normalMaterial;

    [SerializeField] BoidManager boids;

    [SerializeField] GameObject currentLight;
    [SerializeField] GameObject nextLight;

    [SerializeField] SwitchBookTexture nextBook;

    [SerializeField] GameObject dialogueBox;
    [SerializeField] GameObject previousDialogueBox;

    Animator anim;

    [SerializeField] SkinnedMeshRenderer targetRenderer;

    [SerializeField] int boidAmount;

    public bool wasSelected;
    public AudioSource audioSource;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    [ContextMenu("bitch")]
    public void OpenBookAnimation()
    {
        if (!wasSelected)
        {
            wasSelected = true;

            targetRenderer.material = normalMaterial;

            if (nextBook != null)
                nextBook.SwitchTexture();

            ActivateNextLight();

            Invoke(nameof(SummonBoid), 1f);

            anim.SetTrigger("TrOpenBook");
            anim.SetTrigger("TrOpenPages");

            audioSource.Play();

            Invoke(nameof(BookOpenVFX), 15.5f);

            return;
        }

        anim.SetTrigger("TrOpenBook");
        anim.SetTrigger("TrOpenPages");

        audioSource.Play();

        Invoke(nameof(BookOpenVFX), 1f);
    }

    void SummonBoid()
    {
        if (boids != null)
        {
            for (int i = 0; i < boidAmount; i++)
            {
                boids.Add(transform);
            }
        }
    }

    public void CloseBookAnimation()
    {
        CancelInvoke(nameof(BookOpenVFX));

        anim.SetTrigger("TrCloseBook");
        anim.SetTrigger("TrClosePages");

        Invoke(nameof(BookCloseVFX), 1f);
    }

    void BookOpenVFX()
    {
        if (boids != null)
        {
            boids.SetTarget(transform);
            boids.PlayAnimation(transform);
        }
    }

    void BookCloseVFX()
    {
        if (boids != null)
            boids.ResetTarget();
    }

    public void SwitchTexture()
    {
        targetRenderer.material = highlitedMaterial;
    }

    void ActivateNextLight()
    {
        currentLight.SetActive(false);

        if (nextLight != null)
            nextLight.SetActive(true);

        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        if (previousDialogueBox != null)
            previousDialogueBox.SetActive(false);
    }
}
