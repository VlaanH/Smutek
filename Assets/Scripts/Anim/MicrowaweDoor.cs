using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrowaweDoor : MonoBehaviour
{
    
    private bool isOpen=true;
    public Animator animator;
    
    public void ChangeMicrowaveDoor()
    {
        if (Input.GetKeyDown(Settings.SelectedSettings.InteractionKeyCode))
        {
            animator.SetBool("IsOpen", isOpen);
        
            isOpen = !isOpen;
        }
        
    }
}
