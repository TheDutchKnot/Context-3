using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

public class OPen : MonoBehaviour
{
    [SerializeField] XRInteractableAffordanceStateProvider bookTrigger;

    private Animator mAnimator;
    void Start()
    {
        mAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (mAnimator != null && bookTrigger.isSelected)
        {
            mAnimator.ResetTrigger("TrOpenBook");
            mAnimator.ResetTrigger("TrOpenPages");

            mAnimator.SetTrigger("TrOpenBook");
            mAnimator.SetTrigger("TrOpenPages");
        }
    }
}