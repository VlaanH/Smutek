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

        public int DynamicLighting = 0;
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
        
        RightKeyCode = KeyCode.D,
        
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
            BeforeKeyCode = SelectedSettings.BeforeKeyCode,
            SprintKeyCode = SelectedSettings.SprintKeyCode,
            TaskBoxHid =  SelectedSettings.TaskBoxHid,
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
                case 3:
                {
                    keyTexts[i].text = vSyncOptionSelector[SelectedSettings.vSync];
                    break;
                }
                case 4:
                {
                    keyTexts[i].text = dynamicLightingOptionSelector[SelectedSettings.DynamicLighting];
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
    private int _currentLightingState = -1;

    private void InitNonKeySettings(GameSettingsObject settings)
    {
        QualitySettings.vSyncCount = settings.vSync;

        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex == 0) return;
        
        ApplyLightingSettings(settings.DynamicLighting);
    }

    private void ApplyLightingSettings(int dynamicLighting)
    {
        Light[] allLightsObjects = FindObjectsOfType<Light>(true);

        Debug.Log($"Found {allLightsObjects.Length} lights in scene, setting: {dynamicLighting}");
        
        if (_originalLightSettings == null)
        {
            _originalLightSettings = new Dictionary<Light, (LightShadows, float)>();
        }
        
        var keysToRemove = _originalLightSettings.Keys.Where(light => light == null).ToList();
        foreach (var key in keysToRemove)
        {
            _originalLightSettings.Remove(key);
        }

        if (dynamicLighting == 1) // Тени ВЫКЛЮЧЕНЫ
        {
            foreach (var light in allLightsObjects)
            {
                if (light == null) continue;
                if (light.name == "Sun") continue;
                
                // Сохраняем оригинальные настройки только если их еще нет
                if (!_originalLightSettings.ContainsKey(light))
                {
                    _originalLightSettings[light] = (light.shadows, light.intensity);
                }

                light.shadows = LightShadows.None;
            }

            Debug.Log("Dynamic Lighting OFF - Shadows disabled");
        }
        else if (dynamicLighting == 0) // Тени ВКЛЮЧЕНЫ
        {
            foreach (var light in allLightsObjects)
            {
                if (light == null) continue;
                if (light.name == "Sun") continue;
                
                // Восстанавливаем оригинальные настройки
                if (_originalLightSettings.ContainsKey(light))
                {
                    var original = _originalLightSettings[light];
                    light.shadows = original.shadows;
                    light.intensity = original.intensity;
                }
                // Если это новый источник света, сохраняем его текущие параметры
                else
                {
                    _originalLightSettings[light] = (light.shadows, light.intensity);
                }
            }

            Debug.Log("Dynamic Lighting ON - Shadows enabled");
        }

        _currentLightingState = dynamicLighting;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        CleanupLightSettings();
    }

    private void CleanupLightSettings()
    {
        if (_originalLightSettings == null) return;
        
        var keysToRemove = _originalLightSettings.Keys.Where(light => light == null).ToList();
        foreach (var key in keysToRemove)
        {
            _originalLightSettings.Remove(key);
        }

        Debug.Log($"Cleaned up {keysToRemove.Count} destroyed lights from dictionary");
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
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