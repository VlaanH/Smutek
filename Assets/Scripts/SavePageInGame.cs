using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePageInGame : MonoBehaviour
{
    public GameObject SavePage;
    public FirstPersonController FirstPersonController;
    private bool isOpen = false;

    public void OpenSavePage()
    {
        isOpen = true;
        SavePage.SetActive(true);

    }

    public void CloseSavePage()
    {
        isOpen = false;
        SavePage.SetActive(false);
    }


    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (isOpen)
            {
                CloseSavePage();
            }
        }
    }
}
