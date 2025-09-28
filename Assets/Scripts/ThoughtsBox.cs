using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThoughtsBox : MonoBehaviour
{
    public static bool IsStopMemories = false;
    public GameObject ThoughtsBoxGameObject;
    private Text textGameObject;
    private KeyCode nextKeyCode = KeyCode.L;

    public void Start()
    {
        textGameObject = ThoughtsBoxGameObject.GetComponentsInChildren<Text>(true)[1];
    }

    public  void HideThoughtsBox()
    {
        ThoughtsBoxGameObject.SetActive(false);
    }

    public  void SetText(string text)
    {
        textGameObject.text = text;
        ThoughtsBoxGameObject.SetActive(true);
    }
    
    
    void StopMemories()
    {
        ThoughtsBoxGameObject.SetActive(false);
        IsStopMemories = false;
    }
    
    
    void Update()
    {
        
    }

    private List<string> testStory = new List<string>()
    {
        "ОХ какой я жалкий(",
        "Неееет я не жалкий",
        "Как говорила мая бабушка я очень умный, и любознательный",
        "Но она умерла, эх да бабушка, помню много хороших воспоминаний которе она мне подарила",
        "Но она сдохла XD"

    };

    private int i = 0;
    void StartMemories(List<string> story)
    {
        IsStopMemories = true;
        
        i++;
        int storySize = story.Count;

        if (i>storySize)
        {
            i = 0;
            StopMemories();
        }
        else
        {
            ThoughtsBoxGameObject.SetActive(true);
            textGameObject.text = story[i-1] + " " + i; 
        }
        
    }
}
