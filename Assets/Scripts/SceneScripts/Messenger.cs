using System;
using System.Threading;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public enum EMessengerUsers
{
    Sender,
    Recipient
}

[Serializable]
public class MessageObj
{
    public string Text ;
    public EMessengerUsers EMessengerUsersType ;

    
}

[Serializable]
public class MessageObjAndChoiceObj
{
    public string Name;
    
    public int MessagesSent ;
    public List<MessageObj> MessageObjs ;
    
    public bool IsChoice ;
    
    public List<string> Choices ;
    
    public List<string> Branch ;
}


public class Messenger : MonoBehaviour
{
    public List<GameObject> UiElements;
    public GameObject MainBox;
    public DayCycle DayCycal;

    public GameObject PrintsAnimation;
    public ScrollRect ScrollObj;
    public Image UserImageInChat;
    public Text UserTextInChat;

    public List<GameObject> ButtonsChoices;

    public List<GameObject> AddedMessages = new List<GameObject>();

    public bool IsOpen = false;




    public void SetOpenStatus(bool isOpen = false)
    {
        IsOpen = isOpen;
    }


    public void SetPrintsAnimationStatus(bool status )
    {
        PrintsAnimation.SetActive(status);
    }

    private GameObject AddUi(GameObject main,GameObject child)
    {
        //добовление элемента
       var newUiGameObject =  Instantiate(child,main.transform);
       
       
       newUiGameObject.SetActive(true);

       AddedMessages.Add(newUiGameObject);
       
       return newUiGameObject;
    }

    private void ChangeTextInMessages(GameObject child,GameObject main,string text)
    {
        var elements = child.GetComponentsInChildren<Transform>(true);
        var uiText = elements[2].GetComponent<Text>();
        var uiDataText = elements[3].GetComponent<Text>();

        
        uiText.text = text;
        uiDataText.text = DayCycal.TimeOfDay24.ToShortTimeString();
    }

    private void SetNormalTextBlockSize(GameObject child,GameObject main)
    {
        var obj = child.GetComponentsInChildren<Transform>(true);
        var uiImage = obj[1].GetComponent<RectTransform>();;
        var textRectTransform = obj[2].GetComponent<RectTransform>();
        var childRectTransform = child.GetComponent<RectTransform>();
        var uiText = obj[2].GetComponent<Text>();
        
        
        var verticalUiTextElements = textRectTransform.sizeDelta.x;
        
        var linesInText = Math.Ceiling((uiText.text.Length * uiText.fontSize )/ verticalUiTextElements);
        
        var verticalSizeMassageBox = (float)((float)(linesInText * uiText.fontSize) * 0.4);

        
        uiImage.sizeDelta += new Vector2(0, verticalSizeMassageBox);
        
        textRectTransform.sizeDelta += new Vector2(0, verticalSizeMassageBox);
        
        childRectTransform.sizeDelta += new Vector2(0, (float)((verticalSizeMassageBox)*1.3));
        
        
        var heightUiElements = child.GetComponent<RectTransform> ().sizeDelta.y;
        main.GetComponent<RectTransform> ().sizeDelta += new Vector2(0, heightUiElements);
    }



    public void GoToDownScroll()
    {
        ScrollObj.verticalNormalizedPosition = 0;
    }

    public void AddMessage(MessageObj massage)
    {
        var textUi = AddUi(MainBox,UiElements[(int)massage.EMessengerUsersType]);
        

        ChangeTextInMessages(textUi,MainBox,massage.Text);

        SetNormalTextBlockSize(textUi,MainBox);
        
        textUi.SetActive(true);
        
        GoToDownScroll();
    }

    public void ClearAllMessages()
    {
        var mainBoxRectTransform = MainBox.GetComponent<RectTransform>();
        var massages = AddedMessages;
        
            foreach (var massage in massages)
            {
                massage.SetActive(false);
                Destroy (massage, 1);
               
            }
      
        
        AddedMessages.Clear();
        mainBoxRectTransform.sizeDelta = new Vector2(mainBoxRectTransform.sizeDelta.x, 0);
    }


}
