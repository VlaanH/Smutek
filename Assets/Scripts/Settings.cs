using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class Settings : MonoBehaviour
{
    public class KeySettings
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
    }

    public List<Text> keyTexts = new List<Text>();

    public KeySettings _keySettingsBuffer = new KeySettings();
   
    

    public static KeySettings SelectedSettings  = new KeySettings()
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

   

    public void SaveSettings(KeySettings keySettings)
    {
        string applicationSettingsPatch = Application.persistentDataPath+"/"+"settings.json";
        
        var jsonKeySettings = JsonUtility.ToJson(keySettings);
        
        File.WriteAllText(applicationSettingsPatch,jsonKeySettings);

    }
    
    public KeySettings ReadSettings()
    {
        string applicationSettingsPatch = Application.persistentDataPath+"/"+"settings.json";
        
        if (File.Exists(applicationSettingsPatch))
        {
            var jsonKeySettings = JsonUtility.FromJson<KeySettings>(applicationSettingsPatch);
            
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
        SelectedSettings = _keySettingsBuffer;
    }

    public void SetKeyKods(KeySettings keySettings)
    {
        _keySettingsBuffer = new KeySettings()
        {
            JumpKeyCode = SelectedSettings.JumpKeyCode, 
            BackKeyCode = SelectedSettings.BackKeyCode,
            ExitFromInteraction = SelectedSettings.ExitFromInteraction,
            InteractionKeyCode = SelectedSettings.InteractionKeyCode,
            LeftKeyCode = SelectedSettings.LeftKeyCode,
            RightKeyCode = SelectedSettings.RightKeyCode,
            BeforeKeyCode = SelectedSettings.BackKeyCode,
            SprintKeyCode = SelectedSettings.SprintKeyCode
            
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
                

            }
        }
       
        
    }


    void Start()
    {
        SelectedSettings = ReadSettings();
    }

    public void GetPresKeyKod(int id)
    {
        StartCoroutine(GetPressKey(id));

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
