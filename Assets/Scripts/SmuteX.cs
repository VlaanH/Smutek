using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

public class SmuteX : MonoBehaviour
{
    public InputField InputCommandLine;
    public Text MainTextBuffer;
    public FirstPersonController firstPersonController;
    public ChairController MainChair;
    public DayCycle DayCycle;
    
    private const float Hours24InMinutes = 1440;
    
    // Start is called before the first frame update
// var OS = RuntimeInformation.OSArchitecture;
//sh: d: not found
    public void AddTextToCommandBuffer(string text)
    {
        MainTextBuffer.text = MainTextBuffer.text+ "\n" +text ;
    }
  
    public IEnumerator WaitForInputActivation()
    {
        yield return 0;
        InputCommandLine.ActivateInputField();
    }

    public bool FindCommand(string command)
    { 
        return Regex.IsMatch(InputCommandLine.text, "\\b"+command+"\\b");
    }

    public void SendCommand()
    {

        AddTextToCommandBuffer("root@smutek:~# "+InputCommandLine.text);
        
        if (FindCommand("uname"))
        {
            _uname(InputCommandLine.text);
        }
        else if(FindCommand("clear"))
        {
            _clear(InputCommandLine.text);
        }
        else if(FindCommand("tp"))
        {
            _tp(InputCommandLine.text);
        }
        else if (FindCommand("date"))
        {
            _data(InputCommandLine.text);
        }
        else
        {
            nonCommand(InputCommandLine.text);
        }
       
        InputCommandLine.text = "";

        StartCoroutine(WaitForInputActivation());
    }
    
    //////////////////////////////////////////////////////

    private void nonCommand(string text)
    {
        AddTextToCommandBuffer("shmx: "+text+":"+" not found"); //sh: d: not found
    }
    private void _clear(string text)
    { 
        MainTextBuffer.text = "";
    }

    private void _data(string text)
    {
        var commandList = text.Split(' ').ToList();

        
        if (text=="date")
        {
            AddTextToCommandBuffer(DayCycle.TimeOfDay24.ToString("F",new System.Globalization.CultureInfo("en-GB"))); 
        }
        else if(commandList.Count==3)
        {
            if (commandList[1]=="set")
            {
                try
                {
                    float timeint = int.Parse(commandList[2]);

                    
                    if (timeint!=0)
                    {
                        if (timeint<24 & timeint>0)
                        {
                            DayCycle.SetTime((timeint/24)*Hours24InMinutes);
                        }
                    }
                    else
                    {
                        nonCommand(text);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    nonCommand(text);
                }
            }
            else
            {
                nonCommand(text);
            }
           
        }
        else
        {
            nonCommand(text);
        }
    }
    
    private void _uname(string text)
    {
        if (text=="uname -m")
        {
            AddTextToCommandBuffer(RuntimeInformation.OSArchitecture.ToString());
        }
        else if(text=="uname")
        {
            string os="non";
            
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                os = "Linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                os = "Windows";
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                os = "MacOS-OSX";
            }
            
            
            AddTextToCommandBuffer("SmuteX run on "+RuntimeInformation.OSDescription.ToString()+" ("+os+")");
        }
        else if (text == "uname -a")
        {
            string os="non";
            
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                os = "Linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                os = "Windows";
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                os = "MacOS-OSX";
            }
           
            
            
            AddTextToCommandBuffer("SmuteX smutek "+RuntimeInformation.OSDescription.ToString()+" ("+os+") "+
                                   DateTime.Now.ToString("F",new System.Globalization.CultureInfo("en-GB"))
                                   +" " + RuntimeInformation.OSArchitecture.ToString()+" SmuteX/YOUnix");
        }
        else
        {
            nonCommand(text);
        }

    }
    
    
    private void _tp(string text)
    {
        var commandList = text.Split(' ').ToList();
        if (text == "tp list")
        {
            AddTextToCommandBuffer("outside: -o");
        }
        else if(text == "tp -o")
        {
            MainChair.GetUp();
            firstPersonController.transform.position = new Vector3(273, 8, 230);
        }
        else if(text == "tp")
        {
            AddTextToCommandBuffer("tp list | or | to X Y Z");
        }
        else if(commandList.Count>2)
        {
            var x = int.Parse(commandList[1]);
            var y = int.Parse(commandList[2]);
            var z = int.Parse(commandList[3]);
            MainChair.GetUp();
            firstPersonController.transform.position = new Vector3(x, y, z);
        }
        else
        {
            nonCommand(text);
        }
        

       
    }
}
