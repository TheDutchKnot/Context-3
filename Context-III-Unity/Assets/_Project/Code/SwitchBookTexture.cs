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

    Animator anim;

    [SerializeField] SkinnedMeshRenderer targetRenderer;

    public bool wasSelected;

    new void Awake()
    {


        anim = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        if (isSelected && !wasSelected)
        {
            wasSelected = true;

            targetRenderer.material = normalMaterial;

            OpenBookAnimation();

            boids.Add(transform);

            nextBook.SwitchTexture();

            ActivateNextLight();
        }
    }

    public void OpenBookAnimation()
    {
        if (wasSelected) return;

        wasSelected = true;

        targetRenderer.material = normalMaterial;

        boids.Add(transform);

        nextBook.SwitchTexture();

        ActivateNextLight();

        anim.SetTrigger("TrOpenBook");
        anim.SetTrigger("TrOpenPages");
    }

    public void CloseBookAnimation()
    {
        anim.SetTrigger("TrCloseBook");
        anim.SetTrigger("TrClosePages");
    }

    public void SwitchTexture()
    {
        targetRenderer.material = highlitedMaterial;
    }

    void ActivateNextLight()
    {
        currentLight.SetActive(false);
        nextLight.SetActive(true);

        dialogueBox.SetActive(true);
    }
}
