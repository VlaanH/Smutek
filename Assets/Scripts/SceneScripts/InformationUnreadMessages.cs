using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class InformationUnreadMessages :MonoBehaviour
{
    public List<Text> UnreadMessagesUi;
    public List<Text> PrintingUi;

    public static List<bool> IsPrintsList = new List<bool>();
    public static List<int> UnreadMessages = new List<int>();
    
    
    private bool protection = false;
    void Update()
    {
        if (protection==false)
        {
            protection = true;
            SetPrintingAnimation();
            SetUnreadData();
            protection = false;
        }

    }
    


    void SetUnreadData()
    {
        if (InformationUnreadMessages.UnreadMessages!=null)
        {
            for (int i = 0; i < InformationUnreadMessages.UnreadMessages.Count; i++)
            {
                UnreadMessagesUi[i].text = UnreadMessages[i].ToString();
            }
        }
    }

    void SetPrintingAnimation()
    {
        for (int i = 0; i < IsPrintsList.Count; i++)
        {
            if (IsPrintsList[i])
            {
                
                PrintingUi[i].text = PrintsAnimation.textAnimation;
            }
            else
            {
                PrintingUi[i].text = "";
            }
            
        }
        
    }
    
    
}
