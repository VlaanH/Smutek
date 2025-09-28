using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class ScriptСutscenes : MonoBehaviour
{
    public GameObject BackgroundImage;
    public ThoughtsBox ThoughtsBox;
    public AudioSource SelectedMusic;
    public AudioSource SelectedAudio;
    
    
    private Сutscenes MiniPutinHan;
    
    
    public AudioClip miniPutinHanMainAudioSources;
    public List<AudioClip> miniPutinHanAudioClip;
    public List<Sprite> miniPutinHanSpriteSources;

    
    public bool IsStartСutscenes = false;
    private bool IsPrinting = false;
    private int FraimСutscenesNumber = 0;

    private Сutscenes SelectedСutscenes = new Сutscenes();
    private KeyCode activateKeyCode = KeyCode.L;

    

    public void StartMiniPutin()
    {
        MiniPutinHan = new Сutscenes();
        
        SelectedСutscenes = new Сutscenes();
        
        List<FraimСutscenes> fraimСutscenesList = new List<FraimСutscenes>()
        {
            new FraimСutscenes()
            {
                TextSource = "...",
                Image = miniPutinHanSpriteSources[0],
            },
            new FraimСutscenes()
            {
                TextSource = "Знакомое лицо..",
            },
            new FraimСutscenes()
            {
                TextSource = "Чин-гис-хан.. ",
            },
            new FraimСutscenes()
            {
                TextSource = "пена для бритья..",
            },
            new FraimСutscenes()
            {
                TextSource = "Какие неприятные руки.",
                Image = miniPutinHanSpriteSources[1],
            },
            new FraimСutscenes()
            {
                TextSource = "Пена на щеках неприятно пахнет смесью пихты и мыла..",
                AudioClip = miniPutinHanAudioClip[0],
                Image = miniPutinHanSpriteSources[0],
            },
            new FraimСutscenes()
            {
                TextSource = "Где я? ",
            },
            new FraimСutscenes()
            {
                TextSource = "",
                AudioClip = miniPutinHanAudioClip[1],
            },
            new FraimСutscenes()
            {
                TextSource = "–Кто это? Кого то он мне напоминает.. кого то знакомого... или незнакомого..",
                Image = miniPutinHanSpriteSources[2],
            },
            new FraimСutscenes()
            {
                TextSource = "Мне стоит его бояться? Или радоваться? Непонятно.",
            },
            new FraimСutscenes()
            {
                TextSource = "Незнакомец: Не вертитесь, пожалуйста, а то я вам так ненароком ухо отрежу.",
            },
            new FraimСutscenes()
            {
                TextSource = "Почему я его слушаю…",
                Image = miniPutinHanSpriteSources[0],
  
            },
            new FraimСutscenes()
            {
                TextSource = "",
                AudioClip = miniPutinHanAudioClip[2],
                Image = miniPutinHanSpriteSources[3],

            },
        };

        MiniPutinHan.FraimСutscenesList = fraimСutscenesList;
        StartСutscenes(MiniPutinHan);
    }
    
    
    private void Update()
    {
        if (IsStartСutscenes==true && IsPrinting==false)
        {
            if (Input.GetKeyDown(activateKeyCode))
            {
                if (FraimСutscenesNumber < SelectedСutscenes.FraimСutscenesList.Count)
                {
                    SetFrame(SelectedСutscenes.FraimСutscenesList[FraimСutscenesNumber]);
                }
                else
                {
                    StopСutscenes();
                }
                
            }
        }
       
    }

    public class FraimСutscenes
    {

        public AudioClip AudioClip { get; set; }
        
        public Sprite Image { get; set; }

        public string TextSource { get; set; }

    }


    public class Сutscenes
    {
        public List<FraimСutscenes> FraimСutscenesList { get; set; }
   
    }
    
    
    void StartСutscenes(Сutscenes сutscenes)
    {
        IsStartСutscenes = true;
        BackgroundImage.SetActive(true);
        
        SelectedMusic.clip = miniPutinHanMainAudioSources;
        
        SelectedMusic.Play();
        
        SelectedСutscenes = сutscenes;
        SetFrame(сutscenes.FraimСutscenesList[0]);

    }

    void StopСutscenes()
    {
        ThoughtsBox.HideThoughtsBox();
        
        IsStartСutscenes = false;
        
        BackgroundImage.SetActive(false);
        
        SelectedMusic.Stop();

        FraimСutscenesNumber = 0;
    }

    IEnumerator SetPrintingText(string text)
    {
        IsPrinting = true;

        string printingAlreadyText = default;
        
        for (int i = 0; i < text.Length; i++)
        {
            printingAlreadyText += text[i].ToString();
            ThoughtsBox.SetText(printingAlreadyText);
            yield return new WaitForSeconds(0.06f);
            
        }
        IsPrinting = false;
    }
    IEnumerator Damping(float time, Sprite lastImage)
    {
        var backgroundImageImage = BackgroundImage.GetComponent<Image>();
        var startColor = backgroundImageImage.color;

       
        var newColor = new Color();
        newColor = startColor;
        
        var colorFraction = startColor.b;
        Debug.Log(backgroundImageImage.color);
        
        
        for (float i = 0; i < colorFraction; i+=0.01f)
        {
            newColor.r = colorFraction-i;
            newColor.g = colorFraction-i;
            newColor.b = colorFraction-i;
            Debug.Log(i);
            Debug.Log(startColor.r);

            
            backgroundImageImage.color = newColor;
            
            yield return new WaitForSeconds(0.005f*time);
        }

        backgroundImageImage.sprite = lastImage;
        backgroundImageImage.color = startColor;
    }
    void SetFrame(FraimСutscenes fraimСutscenes)
    {
        
        if (fraimСutscenes.Image!=null)
        {
            StartCoroutine(Damping(4,fraimСutscenes.Image));
        }

       
        StartCoroutine(SetPrintingText(fraimСutscenes.TextSource));
        

        if (fraimСutscenes.AudioClip!=null)
        {
            SelectedAudio.clip = fraimСutscenes.AudioClip;
                
            SelectedAudio.Play();
        }

        FraimСutscenesNumber++;

    }
    
    
    
    
}
