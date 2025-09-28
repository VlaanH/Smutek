using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class PrintsAnimation : MonoBehaviour
{
    
    private Text TextPrints;
    private string startText;
    
    public static string textAnimation;
    
    void Start()
    {
        TextPrints = gameObject.GetComponent<Text>();
        startText = TextPrints.text;

        animation();
    }

    private void Update()
    {
        TextPrints.text = startText+textAnimation;
    }


    void animation()
    {
       new Thread(()=>
       {
           
           while (true)
           {
               textAnimation = "";
            
               for (int i = 0; i < 4; i++)
               {
                   Thread.Sleep(300);
                   textAnimation += ".";
               
               }

           }
           
       }).Start();
        
    }
}
