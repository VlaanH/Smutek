using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DistanceSoundControl : MonoBehaviour
{
    public AudioSource AudioSource;
    
    private void OnTriggerEnter(Collider other)
    {
        AudioSource.mute = false;
        Debug.Log("On");
    }

    private void OnTriggerExit(Collider other)
    {
        AudioSource.mute = true;
        Debug.Log("Exit");
       
    }
}
