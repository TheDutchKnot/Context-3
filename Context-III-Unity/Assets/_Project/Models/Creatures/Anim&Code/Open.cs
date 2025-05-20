using UnityEngine;

public class OPen : MonoBehaviour
{
    private Animator mAnimator;
    void Start()
    {
        mAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (mAnimator != null && Input.GetKeyDown(KeyCode.W))
        {
            mAnimator.ResetTrigger("TrOpenBook");
            mAnimator.ResetTrigger("TrOpenPages");

            mAnimator.SetTrigger("TrOpenBook");
            mAnimator.SetTrigger("TrOpenPages");
        }
    }
}