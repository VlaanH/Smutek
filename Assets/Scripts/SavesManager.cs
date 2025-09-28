using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SavesManager : MonoBehaviour
{

    
    [Serializable]
    public class SaveObject
    {
        public List<TransformData> TransformDataList;

        public DateTime DateNow = new DateTime();

        public float TimeSpan;

        public bool IsPcOn;
        
        public List<MessageObjAndChoiceObj> AllBranch ;

        public List<MessageObj> Palichistory;

        public List<MessageObj> AlexHistory;

        public List<MessageObj> VikaHistory;

        public List<string> SelectedBranch;

        public bool IsSitting;

        public Vector3 ChairPostitin;

    }

    [Serializable]
    public class TransformData
    {
        public Vector3 position;
        
        public Quaternion rotation;
        
        public string NameObj;

    }

    

    List<TransformData> GetTransformDataList()
    {
        List<TransformData> transformDataList = new List<TransformData>();
        
        var savesGameObj = GameObject.FindGameObjectsWithTag("SavesObj").ToList();
        
        foreach (var gameObj in savesGameObj)
        {
            TransformData transformData = new TransformData();
            transformData.position = gameObj.transform.position;
            transformData.rotation = gameObj.transform.rotation;
            transformData.NameObj = gameObj.transform.name;
            
            
            transformDataList.Add(transformData);
        }

        return transformDataList;
    }

    public void DeleteSave(string id)
    {
        string savePath = Application.persistentDataPath + "/" + id + ".JsonSav";
        string screenPatch = Application.persistentDataPath + "/" + id + ".png";
        
        Debug.Log(savePath);
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
        
        if (File.Exists(screenPatch))
        {
            File.Delete(screenPatch);
        }
    }

    public void SaveGame(string id)
    {
        
        string savePath = Application.persistentDataPath + "/" + id + ".JsonSav";



        var transformDataList = GetTransformDataList();
        
        var dayCycleManager = GameObject.Find("DayCycleManager").GetComponent<DayCycle>();
        
        var pc = GameObject.Find("PcCase").GetComponent<PcController>();



        var chair = ChairController.sittingOn;

        Vector3 ChairPostitin = new Vector3(); 

        bool isSit = false;

        if (chair!=null)
        {
            isSit = chair.isSit;
            ChairPostitin = chair.GetChairPosition();
        }
        
        
        var save = new SaveObject()
        {
            TransformDataList = transformDataList,
            DateNow = dayCycleManager.TimeOfDay24,
            TimeSpan = dayCycleManager.GetTimeSpan(),
            IsPcOn = pc.IsPcOn,
            IsSitting = isSit,
            ChairPostitin = ChairPostitin
        };

        string json = JsonUtility.ToJson(save);

       
        //Debug.Log(StoryController.HistoryList[2][StoryController.HistoryList[2].Count-1].Text+"<=========>");
       
        
        File.WriteAllText(savePath, json);

        ScreenMem.SaveScreenshotForSave(id);
    }

    public static List<string> GetAllSavesName()
    {
        string savePath = Application.persistentDataPath + "/";

        var filesOnMainDir=  Directory.GetFiles(savePath).ToList();
        List<string> allNames = new List<string>();

        foreach (var file in filesOnMainDir)
        {
            if (Regex.IsMatch(file, "\\bJsonSav\\b"))
            {
                allNames.Add(Path.GetFileName(file));
            }
        }
        
        return allNames;
    }

    private static void LoadStartIncludeObject(SaveObject save)
    {

        var dayCycleManager = GameObject.Find("DayCycleManager").GetComponent<DayCycle>();
        
        var pc = GameObject.Find("PcCase").GetComponent<PcController>();
       

        if (save.IsPcOn)
        {
           
            pc.HardOnPc();
            pc.IsPcOn = save.IsPcOn;
        }
        else
        {
            
            pc.HardOffPc();
            pc.IsPcOn = save.IsPcOn;
        }
        
        dayCycleManager.SetTime(save.TimeSpan);
        
        dayCycleManager.days += dayCycleManager.TimeOfDay;
    }
    

    public static void LoadGame(string id)
    {
        string savePath = Application.persistentDataPath + "/" + id + ".JsonSav";
        
        Debug.Log(savePath);
        
        string json = File.ReadAllText(savePath);
        
        Debug.Log(json);
        
        var save = JsonUtility.FromJson<SaveObject>(json);
        
        
        
        
        var localSavesGameObj = GameObject.FindGameObjectsWithTag("SavesObj").ToList();

        //сортировка по алфавиту
        localSavesGameObj = localSavesGameObj.OrderBy(w => w.transform.name).ToList();

        foreach (var VARIABLE in save.TransformDataList)
        {
            Debug.Log(VARIABLE.NameObj);
        }
        
        
        save.TransformDataList =  save.TransformDataList.OrderBy(w => w.NameObj).ToList();
        
        for (int i = 0; i < localSavesGameObj.Count; i++)
        {
            localSavesGameObj[i].transform.position = save.TransformDataList[i].position;
            localSavesGameObj[i].transform.rotation = save.TransformDataList[i].rotation;
        }
   

        if (save.IsSitting)
        {
            ChairController.GetChairAtPosition(save.ChairPostitin).Sit();
        }
        
        
        LoadStartIncludeObject(save);
        
        
    }

}
