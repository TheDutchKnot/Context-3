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

    protected virtual void Update() { }

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

            Invoke(nameof(SummonBoid), 1.5f);

            anim.SetTrigger("TrOpenBook");
            anim.SetTrigger("TrOpenPages");

            audioSource.Play();

            return;
        }

        anim.SetTrigger("TrOpenBook");
        anim.SetTrigger("TrOpenPages");

        audioSource.Play();

        if (boids != null)
            boids.SetTarget(transform);
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
        anim.SetTrigger("TrCloseBook");
        anim.SetTrigger("TrClosePages");

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

        dialogueBox.SetActive(true);
        previousDialogueBox.SetActive(false);
    }
}
