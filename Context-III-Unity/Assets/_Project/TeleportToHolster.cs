using UnityEngine;

public class TeleportToHolster : MonoBehaviour
{
    [SerializeField] Transform item;
    [SerializeField] float waitTime;

    void Start()
    {
        Invoke(nameof(Teleport), waitTime);
    }

    void Teleport() => item.position = transform.position;
}
