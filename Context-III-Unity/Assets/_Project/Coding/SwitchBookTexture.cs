using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

public class SwitchBookTexture : XRInteractableAffordanceStateProvider
{
    [Header("BEEP BOOP")]
    [SerializeField] Material highlitedMaterial;
    [SerializeField] Material normalMaterial;

    [SerializeField] GameObject currentLight;
    [SerializeField] GameObject nextLight;

    [SerializeField] SwitchBookTexture nextBook;

    MeshRenderer rend;

    bool wasSelected;

    void Awake()
    {
        rend = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (isSelected && !wasSelected)
        {
            wasSelected = true;

            rend.material = normalMaterial;

            nextBook.SwitchTexture();

            ActivateNextLight();
        }
    }

    public void SwitchTexture()
    {
        rend.material = highlitedMaterial;
    }

    void ActivateNextLight()
    {
        currentLight.SetActive(false);
        nextLight.SetActive(true);
    }
}
