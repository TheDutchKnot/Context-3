using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

public class ActivateNextBook : XRInteractableAffordanceStateProvider
{
    [SerializeField] GameObject currentLight;
    [SerializeField] GameObject nextLight;
    
    // Update is called once per frame
    void Update()
    {
        if(isHovered)
        {

        }

      //when player picks up  the light thats on, ActivateNextLight
    }

    void ActivateNextLight()
    {
        currentLight.SetActive(false);
        nextLight.SetActive(true);
    }
}
