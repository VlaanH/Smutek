using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PcController : MonoBehaviour
{
    public MonitorController MonitorController;
    public List<FanController> FanController;
    
    public AudioSource audioSource;
    public bool IsPcOn = false;
    private bool IsBlock = false;
    
   

    IEnumerator SmoothDecay(bool isUp)
    {
        IsBlock = true;
        float startVolume = audioSource.volume;
        
        if (isUp!=true)
        {
            audioSource.mute = false;
        }
        
        for (int i = 0; i < 100; i++)
        {
            if (isUp)
            {
                audioSource.volume = startVolume-(float)i*10 / 1000;
            }
            else
            {
                audioSource.volume = (float)i*10 / 1000;
            }
           
            yield return new WaitForSeconds(0.01f);
            

        }
        if (isUp)
        {
            audioSource.mute = true;
        }
        IsBlock = false;
    }

    public void OffPc()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            audioSource.mute = true;
            
            MonitorController.HideAllScenes();
            
            foreach (var fan in FanController)
                StartCoroutine(fan.SlowDownFan());

            StartCoroutine(SmoothDecay(true));

            
            IsPcOn = false;
            
        }
        
    }


    
    public void OnPc()
    {
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            audioSource.mute = false;

            StartCoroutine(SmoothDecay(false));
            
            foreach (var fan in FanController)
                fan.OnFan();
            
            MonitorController.InTheMainMenu();
            
            
            IsPcOn = true;
        }
    }
    
    
    public void HardOffPc()
    {
        MonitorController.HideAllScenes();
        IsPcOn = false;
    }

    public void HardOnPc()
    {
        
        StartCoroutine(SmoothDecay(false));
        
        foreach (var fan in FanController)
            fan.OnFan();
            
        MonitorController.InTheMainMenu();

        
        IsPcOn = true;
    }


    public void ChangePcStatus()
    {
        
        if (IsBlock==false)
        {
            if (IsPcOn)
                OffPc();
            else 
                OnPc();
        }
       
            
    }


}
