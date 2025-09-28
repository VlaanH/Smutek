using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SavePage : MonoBehaviour
{

    public SavesManager SavesManager;
    
    
    public bool IsInMenu = true;
    
    public int pageId = 1;

    public Text PageText;
    
    public List<GameObject> SavesBoxes = new List<GameObject>();
    
    
    
    
    void Start()
    {
        SetAllSavesBox();
    }




    bool IsSaveVoid(List<string> saveName,int id)
    {
        
        try
        {
            foreach (var save in saveName)
            {
                var idName= int.Parse(Path.GetFileNameWithoutExtension(save));
                if (idName==id)
                {
                    return false;
                }
            }
            return true;
        }
        catch (Exception)
        {
            // ignored
        }

        return true;
    }


    public void DisableAllAdd()
    {
        foreach (var saveBox in SavesBoxes)
        {
            int addId = 8;

            var gameObjs =  saveBox.GetComponentsInChildren<Transform>(true);
            
            var add = gameObjs[addId];
            
            var addButton = add.GetComponent<Button>();
            
            addButton.interactable= false;
            
        }
        
       
    }


    public void SetAllSavesBox()
    {
       // DisableAllAdd();
        
        var allSaves =  SavesManager.GetAllSavesName();
        var numberOfBoxes = SavesBoxes.Count;
        int index = 0;
        for (int i = (pageId*numberOfBoxes)-numberOfBoxes; i < numberOfBoxes*pageId; i++)
        {

            Debug.Log(i +" | "+ numberOfBoxes*pageId+" | "+IsSaveVoid(allSaves,i));
            if (IsSaveVoid(allSaves,i)==false)
            {
                SetSaveBoxData(false,index,i);
            }
            else
            {
                SetSaveBoxData(true,index,i);
               
            }
            
            index++;

        }
        Debug.ClearDeveloperConsole();

    }

    public void NextPage()
    {
        if (pageId<10)
        {
            pageId++;
            SetAllSavesBox();
            PageText.text = pageId.ToString();
        }
      
    }
    public void BackPage()
    {
        if (pageId>1)
        {
            pageId--;
            SetAllSavesBox();
            PageText.text = pageId.ToString();
        }
      
    }

    public void SetSaveBoxData(bool isVoidSave,int saveBoxId,int id)
    {
        int controlId = 1;
        int addId = 8;

       var gameObjs = SavesBoxes[saveBoxId].GetComponentsInChildren<Transform>(true);

       var control = gameObjs[controlId];
       var add = gameObjs[addId];


 
       
       var addButton = add.GetComponent<Button>();
       
       if (isVoidSave)
       {
           
           if (IsInMenu==true)
           {
               
               addButton.interactable = false;
           }
           else
           {
               addButton.interactable = true;
               
               addButton.onClick.RemoveAllListeners();
               addButton.onClick.AddListener(delegate
               {
                   SavesManager.SaveGame(id.ToString());
                   SetAllSavesBox();
               });
           }
           
           control.gameObject.SetActive(false);
           add.gameObject.SetActive(true);
           
       }
       else
       {
          
           
           control.gameObject.SetActive(true);
           add.gameObject.SetActive(false);
           
           var image = control.GetComponentsInChildren<Transform>(true)[2];
           
           var imgBlock = image.GetComponent<Image>();

           var controlButtons = image.GetComponentsInChildren<Transform>(true);
           
           var loaudingButton = controlButtons[1].GetComponent<Button>();
           
           var deleteButton = controlButtons[3].GetComponent<Button>();

           loaudingButton.onClick.RemoveAllListeners();
           loaudingButton.onClick.AddListener(delegate
           {
               MainMenuConroller.LoadSaveAndScene(id.ToString());

           });
           
           
           deleteButton.onClick.RemoveAllListeners();
           deleteButton.onClick.AddListener(delegate
           {
               SavesManager.DeleteSave(id.ToString());
               SetAllSavesBox();

           });


           var filePatch = ScreenMem.GetScreenshotForId(id.ToString());

           if (filePatch!=null)
           {
               Texture2D loadedTexture = ScreenMem.LoadTextureFromFile(filePatch);
           
               imgBlock.sprite = ScreenMem.SpriteFromTexture(loadedTexture);
           }
           
           
           
           
       }
       
    }
    
    
    
    
    
    
}
