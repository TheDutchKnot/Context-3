using UnityEngine;

public class BackToInv : MonoBehaviour
{
    [SerializeField] Transform socket;

    public void ReturnToInventory()
    {
        Invoke(nameof(Return), 1f);
    }

    void Return() => transform.position = socket.position;
}
