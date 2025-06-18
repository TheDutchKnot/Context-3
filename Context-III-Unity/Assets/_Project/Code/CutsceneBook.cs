using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

public class CutsceneBook : XRInteractableAffordanceStateProvider
{
    Animator anim;
    public AudioSource audioSource;

    bool wasSelected;


    new void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    protected override void Update()
    {
        base.Update();

        if (isSelected && !wasSelected)
        {
            wasSelected = true;

            OpenBookAnimation();
        }
    }

    public void OpenBookAnimation()
    {
        if (wasSelected) return;

        wasSelected = true;

        anim.SetTrigger("TrOpenBook");
        anim.SetTrigger("TrOpenPages");

        audioSource.Play();
    }

    public void CloseBookAnimation()
    {
        anim.SetTrigger("TrCloseBook");
        anim.SetTrigger("TrClosePages");

        audioSource.Play();
    }
}
