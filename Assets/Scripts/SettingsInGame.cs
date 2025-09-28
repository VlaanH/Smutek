using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsInGame : MonoBehaviour
{
    public Settings Settings;
    private bool isOpen = false;
    
    public GameObject SettingsPage;
    
    public void OpenSettings()
    {
        Settings.SetKeyKods(Settings.SelectedSettings);
        
        SettingsPage.SetActive(true);
        isOpen = true;
    }
    public void CloseSettings()
    {
        Settings._keySettingsBuffer = Settings.SelectedSettings;
        
        SettingsPage.SetActive(false);
        isOpen = false;
    }
    
    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (isOpen)
            {
                CloseSettings();
            }
        }
    }
}
