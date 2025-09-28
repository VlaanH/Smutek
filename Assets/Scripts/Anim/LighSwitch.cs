using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LighSwitch : MonoBehaviour
{
    private bool isPress=false;
    public Animator animator;

    public GameObject lightLamp;
    public Renderer emissionMaterialRenderer;

    public void ChangeSwitchPosition()
    {
        
        if (Input.GetKeyDown(Settings.SelectedSettings.InteractionKeyCode))
        {
            animator.SetBool("IsPress",isPress);
            
            if (isPress)
                emissionMaterialRenderer.material.EnableKeyword("_EMISSION"); 
            
            else 
                emissionMaterialRenderer.material.DisableKeyword("_EMISSION");
            
            lightLamp.SetActive(isPress);
            
            isPress = !isPress;
        }
    }
}
