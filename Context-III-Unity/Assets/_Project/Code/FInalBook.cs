using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;


public class FInalBook : SwitchBookTexture
{
    [Header("collider")]
    [SerializeField] Collider col;

    bool wasSelected;

    void Awake()
    {
        col = GetComponent<Collider>();
    }

    void Update()
    {
        if (isSelected && !wasSelected)
        {
            wasSelected = true;
            col.isTrigger = true;
        }
    }
}
