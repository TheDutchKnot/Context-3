using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;


public class FInalBook : SwitchBookTexture
{
    [Header("collider")]
    [SerializeField] Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
    }

    void Update()
    {
        if (isSelected && !wasSelected)
        {
            col.isTrigger = true;
        }
    }
}
