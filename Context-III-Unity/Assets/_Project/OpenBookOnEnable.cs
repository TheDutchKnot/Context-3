using UnityEngine;

public class OpenBookOnEnable : MonoBehaviour
{
    [SerializeField] OPen book;

    private void OnEnable()
    {
        book.OpenBook();
    }
}
