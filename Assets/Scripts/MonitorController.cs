using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MonitorController : MonoBehaviour
{
    [FormerlySerializedAs("personC")] public FirstPersonController personController;
    public List<GameObject> scenes;
    public ChairController MainChairController;
    public TasksController TasksController;

    public List<GameObject> Pages;
    
    public bool IsUsed = false;

    public void InTheMainMenu()
    {
        HideAllScenes();
        
        scenes[0].gameObject.SetActive(true);
        
    }

    public void HideAllScenes()
    {
        foreach (var scene in scenes)
        {
            scene.gameObject.SetActive(false);
        }
    }

    public void OpenScene(int id)
    {
        HideAllScenes();


        scenes[id].gameObject.SetActive(true);
    }

    public void PageOnSceneHide(int page)
    {
        
        Pages[page].SetActive(false);
       
    }
    public void PageOnSceneOpen(int page)
    {
        
        Pages[page].SetActive(true);
      
    }

    public void ChangeMonitorUsage(bool isUsed)
    {
        personController.cameraCanMove = !isUsed;
        personController.lockCursor = !isUsed;
    
     
        personController.crosshair = !isUsed; 
        
        // Принудительно устанавливаем состояние курсора
        if (isUsed)
        {
            // Монитор используется - курсор видимый и разблокированный
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Монитор не используется - возвращаем к игровому режиму
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (MainChairController.isSit == false)
        {
            personController.playerCanMove = !isUsed;
            personController.enableJump = !isUsed;
        }
    
        TasksController.IsTaskBoxesHide(!isUsed);
        IsUsed = isUsed;
    }

    public void UseMonitor()
    {
        if (Input.GetKeyDown(Settings.SelectedSettings.InteractionKeyCode))
        {
            if (IsUsed == false)
            {
                ChangeMonitorUsage(true);
            }
        }
    }

    public void DoNotUseMonitor()
    { 
        if (IsUsed==true)
            ChangeMonitorUsage(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(Settings.SelectedSettings.ExitFromInteraction))
        {
            DoNotUseMonitor();
        }
    }
}
