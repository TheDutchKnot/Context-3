using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;


public class FInalBook : SwitchBookTexture
{
    [Header("collider")]
    [SerializeField] Collider col;

    protected override void Awake()
    {
        base.Awake();

        col = GetComponent<Collider>();
    }

    protected override void Update()
    {
        base.Update();

        if (isSelected && !wasSelected)
        {
            col.isTrigger = true;
        }
    }
}
