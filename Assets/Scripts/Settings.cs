using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Settings : MonoBehaviour
{

    public List<Light> AllLightsObjectse = new List<Light>();
    
    public List<string> vSyncOptionSelector;
    
    public List<string> dynamicLightingOptionSelector;

    

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

        public int DynamicLighting = 1;
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
            vSync = SelectedSettings.vSync,
            DynamicLighting = SelectedSettings.DynamicLighting
            
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
                case (int)4:
                {
                    keyTexts[i].text =dynamicLightingOptionSelector[SelectedSettings.DynamicLighting] ;
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
    

    private Dictionary<Light, (LightShadows shadows, float intensity)> _originalLightSettings;
    private Light[] AllLightsObjects;


    private void InitNonKeySettings(GameSettingsObject settings)
    {
        QualitySettings.vSyncCount = settings.vSync;

        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex == 0) return; // Пропускаем главное меню
        
        ApplyLightingSettings(settings.DynamicLighting);
      
    }

    private void ApplyLightingSettings(int dynamicLighting)
    {
        // Получаем все источники света в сцене
        AllLightsObjects = Resources.FindObjectsOfTypeAll<Light>()
            .Where(light => light.gameObject.scene.IsValid())
            .ToArray();

        Debug.Log($"Found {AllLightsObjects.Length} lights in scene " + dynamicLighting.ToString());

        if (dynamicLighting == 1)
        {
            // Сохраняем оригинальные настройки при первом переключении
            if (_originalLightSettings == null)
            {
                _originalLightSettings = new Dictionary<Light, (LightShadows, float)>();
            }

            // Отключаем тени
            foreach (var light in AllLightsObjects)
            {
                // Сохраняем только если ещё не сохранили
                if (!_originalLightSettings.ContainsKey(light))
                {
                    _originalLightSettings[light] = (light.shadows, light.intensity);
                
                }

                light.shadows = LightShadows.None;
            }

            Debug.Log("DynamicLighting OFF - Shadows disabled");
        }
        else if (dynamicLighting == 0)
        {
            // Восстанавливаем динамическое освещение с тенями
            foreach (var light in AllLightsObjects)
            {
                if (_originalLightSettings != null && _originalLightSettings.ContainsKey(light))
                {
                    light.shadows = _originalLightSettings[light].shadows;
                    light.intensity = _originalLightSettings[light].intensity;
                }
            }

            Debug.Log("DynamicLighting ON - Shadows enabled");
        }
    }

    public void GetPresKeyKod(int id)
    {
        StartCoroutine(GetPressKey(id));

    }
    public void SelectOptionsVSync(int id)
    {
        _keySettingsBuffer.vSync = SelectOptions(id,vSyncOptionSelector);
    }
    
    public void SelectOptionsVSyncDynamicLighting(int id)
    {
        _keySettingsBuffer.DynamicLighting = SelectOptions(id,dynamicLightingOptionSelector);
    }
    
    private int SelectOptions(int id,List<string> optionSelector)
    {
       
        int IdSelected = 0;
            
        for (int i = 0; i < optionSelector.Count; i++)
        {
            if ( keyTexts[id].text==optionSelector[i])
            {
                IdSelected = i;
            }
               
        }

        var nextId = IdSelected + 1;
            
        if (nextId==optionSelector.Count)
        {
            nextId = 0;
        }
        keyTexts[id].text = optionSelector[nextId];

        Debug.Log(nextId);
        
        return nextId;
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
