using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;


public class FInalBook : MonoBehaviour
{
    [Header("collider")]
    [SerializeField] Collider col;

    SwitchBookTexture refScript;

    void Awake()
    {
        refScript = GetComponent<SwitchBookTexture>();
    }

    void Update()
    {
        if (refScript.wasSelected)
        {
            col.isTrigger = true;
        }
    }
}
