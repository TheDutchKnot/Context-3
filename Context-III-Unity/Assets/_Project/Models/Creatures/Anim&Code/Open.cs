using UnityEngine;

public class OPen : MonoBehaviour
{
    [SerializeField]
    Animator mAnimator;

    void Update()
    {
        if (mAnimator != null && Input.GetKeyDown(KeyCode.W))
        {
            OpenBook();
        }
    }

    [ContextMenu("OpenBook")]
    public void OpenBook()
    {
        mAnimator.ResetTrigger("TrOpenBook");
        mAnimator.ResetTrigger("TrOpenPages");

        mAnimator.SetTrigger("TrOpenBook");
        mAnimator.SetTrigger("TrOpenPages");
    }
}