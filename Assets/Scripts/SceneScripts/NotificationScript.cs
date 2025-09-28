using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NotificationScript : MonoBehaviour
{
    
    public class AlertObject
    { 
        public GameObject AlertListUi;

       
    }
    

    
    public GameObject AlertExemplar;
    public GameObject AlertContener;

    public List<AlertObject> AlertList = new List<AlertObject>();
    

    private GameObject AddUi(GameObject main,GameObject child)
    {
        //добовление элемента
        var newUiGameObject =  Instantiate(child,main.transform);
        
        return newUiGameObject;
    }

    public void DestroyAlert(AlertObject alertObject)
    {
        alertObject.AlertListUi.SetActive(false);
        Destroy(alertObject.AlertListUi);
        AlertList.Remove(alertObject); 
    }


    


    public void AddAlert( string text)
    {
        
        var alertUiObject = AddUi(AlertContener,AlertExemplar);


        var alertObject = new AlertObject()
        {
            AlertListUi = alertUiObject,
           
        };
        
        var elements = alertUiObject.GetComponentsInChildren<Transform>(true);
        
        
        
        
        elements[4].GetComponent<Text>().text = text;
        
        elements[5].GetComponent<Button>().onClick.AddListener(delegate
        {
            DestroyAlert(alertObject);
        });
        
        AlertList.Add(alertObject);
        
        alertObject.AlertListUi.SetActive(true);
    }
}
