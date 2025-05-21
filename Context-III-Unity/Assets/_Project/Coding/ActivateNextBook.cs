using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

public class ActivateNextBook : XRInteractableAffordanceStateProvider
{
    [SerializeField] GameObject currentLight;
    [SerializeField] GameObject nextLight;

    bool wasSelected;

    // Update is called once per frame
    void Update()
    {
        if(isSelected && !wasSelected)
        {
            wasSelected = true;

            ActivateNextLight();
        }

      //when player picks up  the light thats on, ActivateNextLight
    }

    void ActivateNextLight()
    {
        currentLight.SetActive(false);
        nextLight.SetActive(true);
    }
}