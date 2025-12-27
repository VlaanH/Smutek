using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class Settings : MonoBehaviour
{
    
    public List<string> vSyncOptionSelector;

    enum EOptionSelectors
    {
        VSync,
        DynamicLighting
    }

    public class GameSettingsObject
    {
        public KeyCode JumpKeyCode;
        
        public KeyCode InteractionKeyCode;

        public KeyCode ExitFromInteraction;

        public KeyCode BeforeKeyCode;
        
        public KeyCode BackKeyCode;
        
        public KeyCode LeftKeyCode;
        
        public KeyCode RightKeyCode;
        
        public KeyCode SprintKeyCode;

        public KeyCode TaskBoxHid;

        public int vSync = 0;

    
    }

    public List<Text> keyTexts = new List<Text>();
    
    public GameSettingsObject _keySettingsBuffer = new GameSettingsObject();
    

    public static GameSettingsObject SelectedSettings  = new GameSettingsObject()
    {
        JumpKeyCode = KeyCode.Space,
        
        InteractionKeyCode = KeyCode.E,
        
        ExitFromInteraction = KeyCode.LeftShift,
        
        BeforeKeyCode = KeyCode.W,
        
        BackKeyCode = KeyCode.S,
        
        LeftKeyCode = KeyCode.A,
        
        RightKeyCode = KeyCode.R,
        
        SprintKeyCode = KeyCode.LeftShift,
        
        TaskBoxHid = KeyCode.I
    };

   

    public void SaveSettings(GameSettingsObject keySettings)
    {
        string applicationSettingsPatch = Application.persistentDataPath+"/"+"settings.json";
        
        var jsonKeySettings = JsonUtility.ToJson(keySettings);
        
        File.WriteAllText(applicationSettingsPatch,jsonKeySettings);

    }
    
    public GameSettingsObject ReadSettings()
    {
        string applicationSettingsPatch = Application.persistentDataPath+"/"+"settings.json";

        Debug.Log(applicationSettingsPatch);
        if (File.Exists(applicationSettingsPatch))
        {
            var json = File.ReadAllText(applicationSettingsPatch);

            var jsonKeySettings = JsonUtility.FromJson<GameSettingsObject>(json);
            
            return jsonKeySettings;
        }
        else
        {
            return SelectedSettings;
        }
    }

    enum EKodCods
    {
        InteractionKeyCode,
        BeforeKeyCode,
        BackKeyCode
    }

    public void SaveSettings()
    {
        SaveSettings(_keySettingsBuffer);
        
        InitNonKeySettings(_keySettingsBuffer);
        
        SelectedSettings = _keySettingsBuffer;
    }

    public void SetKeyKods(GameSettingsObject keySettings)
    {
        _keySettingsBuffer = new GameSettingsObject()
        {
            JumpKeyCode = SelectedSettings.JumpKeyCode, 
            BackKeyCode = SelectedSettings.BackKeyCode,
            ExitFromInteraction = SelectedSettings.ExitFromInteraction,
            InteractionKeyCode = SelectedSettings.InteractionKeyCode,
            LeftKeyCode = SelectedSettings.LeftKeyCode,
            RightKeyCode = SelectedSettings.RightKeyCode,
            BeforeKeyCode = SelectedSettings.BackKeyCode,
            SprintKeyCode = SelectedSettings.SprintKeyCode,
            vSync = SelectedSettings.vSync
            
        };
        
        for (int i = 0; i < keyTexts.Count; i++)
        {
            switch (i)
            {
                case (int)EKodCods.InteractionKeyCode:
                {
                    keyTexts[i].text = keySettings.InteractionKeyCode.ToString().ToUpper();
                    break;
                }
                case (int)EKodCods.BeforeKeyCode:
                {
                    keyTexts[i].text = keySettings.BeforeKeyCode.ToString().ToUpper();
                    break;
                }
                case (int)EKodCods.BackKeyCode:
                {
                    keyTexts[i].text = keySettings.BackKeyCode.ToString().ToUpper();
                    break;
                }
                case (int)3:
                {
                    keyTexts[i].text =vSyncOptionSelector[SelectedSettings.vSync] ;
                    break;
                }

            }
        }
       
        
    }


    void Start()
    {
        SelectedSettings = ReadSettings();
        
        InitNonKeySettings(SelectedSettings);
    }

    private void InitNonKeySettings(GameSettingsObject settings)
    {
        QualitySettings.vSyncCount = settings.vSync;

    }

    public void GetPresKeyKod(int id)
    {
        StartCoroutine(GetPressKey(id));

    }
    public void SelectOptionsVSync(int id)
    {
        var optionsId = (int)EOptionSelectors.VSync;
        SelectOptions(id,optionsId);

    }
    
    private void SelectOptions(int id,int optionsId)
    {
        if (optionsId==(int)EOptionSelectors.VSync)
        {
            int IdSelected = 0;
            
            for (int i = 0; i < vSyncOptionSelector.Count; i++)
            {
                if ( keyTexts[id].text==vSyncOptionSelector[i])
                {
                    IdSelected = i;
                }
               
            }

            var nextId = IdSelected + 1;
            
            if (nextId==vSyncOptionSelector.Count)
            {
                nextId = 0;
            }
            keyTexts[id].text = vSyncOptionSelector[nextId];

            Debug.Log(nextId);
            _keySettingsBuffer.vSync = nextId;
        }
        
    }

    
    private IEnumerator GetPressKey(int id)
    {
        PresKeySelect = true;
        keyTexts[id].text = "--";
        while (PresKeySelect==true)
        {
           
            yield return new WaitForSeconds(0.1f);
            
        }
        keyTexts[id].text = SelectedKey.ToString().ToUpper();
        Debug.Log(SelectedKey.ToString());
        
        
        switch (id)
        {
            case (int)EKodCods.InteractionKeyCode:
            {
                _keySettingsBuffer.InteractionKeyCode = SelectedKey;
                break;
            }
            case (int)EKodCods.BeforeKeyCode:
            {
                _keySettingsBuffer.BeforeKeyCode = SelectedKey;
                break;
            }
            case (int)EKodCods.BackKeyCode:
            {
                _keySettingsBuffer.BackKeyCode = SelectedKey;
                break;
            }
                

        }

    }

    private bool PresKeySelect = false;

    private KeyCode SelectedKey = default;

    private readonly Array keyCodes = Enum.GetValues(typeof(KeyCode));
    void Update()
    {
        if (PresKeySelect==true)
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode keyCode in keyCodes)
                {
                    if (Input.GetKey(keyCode)) 
                    {
                        SelectedKey = keyCode;
                        
                        PresKeySelect = false;
                    }
                }
            }
            
        }
    }
}
