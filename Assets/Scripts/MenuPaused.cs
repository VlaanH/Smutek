using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPaused : MonoBehaviour
{
    public static bool menuBlock = false;
    
    private bool isMenuPaused = false;
    
    public GameObject menuPausedGameObject;
    public FirstPersonController personController;

    public MonitorController monitorController;
    
    public ChairController MainChairController;

    public BedController BedController;
    
 
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            
            ScreenMem.TakeScreenshot(Camera.main);
            ChangeMenuStatus();

        }
    }

    public void ChangeMenuStatus()
    {
        Debug.Log(menuBlock);
        if (menuBlock == false)
        {
            isMenuPaused = !isMenuPaused;

            if (BedController.IsPlayerLaying()==false)
            {
                if (monitorController.IsUsed==false)
                {
                    personController.crosshair = !personController.crosshair;
        
                    personController.lockCursor = !isMenuPaused;
                    personController.cameraCanMove = !isMenuPaused;
                }

                if (MainChairController.isSit==false)
                {
                    personController.playerCanMove = !personController.playerCanMove;
        
                    personController.enableJump = !personController.enableJump;
        
                    personController.isWalking = false;
                }
            }
        
        
            menuPausedGameObject.SetActive(isMenuPaused);
        }
        
      
    }

}
