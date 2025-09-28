using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlotGlobalController : MonoBehaviour
{
    public WakeUpBlink WakeUpBlinkObj;
    public FaintingEffect FaintingEffectObj;
    public TasksController TasksControllerObj;
    public HintsBox HintsBoxObj;
    public BackgroundMusic BackgroundMusicObj;
    public PcController PcControllerObj;
    public ContractManager ContractManagerObj;

    public DoorController HolodilnikObj;
    public MovingObjects PacketObj;
    public BedController BedControllerObj;
   

    public Transform CemeterySceneCameraPositionObj;
    public Transform MorningStartPositionObj;
    public FirstPersonController PlayerObj;
    
    public AudioSource CemeteryAudioSourceObj;
    public AudioClip CemeteryAudio;
    
    public AudioSource CemeteryPlayerAudioSourceObj;
    public AudioClip CemeteryPlayerAudio;

    public AudioSource DoorKnockSorce;
    public AudioClip DoorKnockClip;

    public InteractableObject pills;
    public InteractableObject chips;
    public Door doorOnHome;
    
    public List<GameObject> Props;
    public GameObject PcBlock;
    private void Start()
    {
        StartCoroutine(StartScene());
    }

    
    void Teleport(GameObject player, Transform teleportPosition)
    {
        player.transform.position = teleportPosition.position;
        player.transform.rotation = teleportPosition.rotation;
    }


    void ClearProps()
    {
        HolodilnikObj.ForcedlyChangeDoorPosition();
        foreach (var prop in Props)
        {
            prop.SetActive(false);
        }
    }
    void CemeteryPlayerAudioStart()
    {
        // запуск звука обморока
        if (CemeteryPlayerAudioSourceObj != null && CemeteryPlayerAudio != null)
        {
            CemeteryPlayerAudioSourceObj.clip = CemeteryPlayerAudio;
            CemeteryPlayerAudioSourceObj.loop = false;
            CemeteryPlayerAudioSourceObj.Play();
        }
    }
    void CemeteryAudioStart()
    {
        // запуск звука обморока
        if (CemeteryAudioSourceObj != null && CemeteryAudio != null)
        {
            CemeteryAudioSourceObj.clip = CemeteryAudio;
            CemeteryAudioSourceObj.loop = false;
            CemeteryAudioSourceObj.Play();
        }
    }

    void KnockAudioStart()
    {
        // запуск звука обморока
        if (DoorKnockSorce != null && DoorKnockClip != null)
        {
            DoorKnockSorce.clip = DoorKnockClip;
            DoorKnockSorce.loop = true;
            DoorKnockSorce.Play();
        }
    }
    
    //1 Сцена началась
    public IEnumerator  StartScene()
    {
        BackgroundMusicObj.StopMusic();
        
        //Teleport(PlayerObj,CemeterySceneCameraPositionObj);
        MenuPaused.menuBlock = true;
        
        //запуск эфектов при тошноте и обмораке
        PlayerObj.FreezingPlayer(true);
        WakeUpBlinkObj.RestartWakeUpEffect();
        
        yield return  FaintingEffectObj.TriggerFaint();
        
        
        //ждам несколько секунд
        
        yield return new WaitForSeconds(1);
        TasksControllerObj.SetTask(new TasksController.TaskObj(){Description = "Подими упавший пакет", Title = "Взять пакет"}, 0);
        yield return new WaitForSeconds(3);
        PlayerObj.FreezingPlayer(false);
        MenuPaused.menuBlock = false;
        
        //первое задание 
       
        
        
        
        yield return new WaitUntil(() => PacketObj.ObjectIsTaken());
        
        yield return TasksControllerObj.CompleteTask(0);
        
        HintsBoxObj.SetHintData(2);
        yield return new WaitForSeconds(1);
        
        TasksControllerObj.SetTask(new TasksController.TaskObj(){Description = "Подезд 2, этаж 4", Title = "Положи продукты в холодильник"}, 0);
        yield return new WaitForSeconds(2);
        yield return new WaitUntil(() => HolodilnikObj.IsOpen());

        
        //начало сцены с гробом
        
        PlayerObj.FreezingPlayer(true);
        PacketObj.ThrowAway();
        TasksControllerObj.TaskBoxesActive(false);
        yield return TasksControllerObj.CompleteTask(0);



        

        Teleport(PlayerObj.gameObject,CemeterySceneCameraPositionObj);

        WakeUpBlinkObj.RestartWakeUpEffect();
        
        CemeteryAudioStart();
        
        yield return  FaintingEffectObj.TriggerFaint();


        yield return new WaitForSeconds(31);
        CemeteryPlayerAudioStart();
        //ждам несколько секунд
        yield return new WaitForSeconds(28);

        //чёрный экран
        WakeUpBlinkObj.SetBlackEye();
        
       
        Teleport(PlayerObj.gameObject,MorningStartPositionObj);
        //лежим  в кровати
        ClearProps();
        yield return BedControllerObj.LayDownOnBed();
        
        //Косвено убераем чёрный экран
        WakeUpBlinkObj.RestartWakeUpEffect();
        yield return FaintingEffectObj.TriggerFaint(false);
        yield return new WaitForSeconds(1);
        
        
        WakeUpBlinkObj.RestartWakeUpEffect();
     
        yield return new WaitForSeconds(2);
        
        
        HintsBoxObj.SetHintData(3);
        
        BackgroundMusicObj.PlayMusic();
        
        
        TasksControllerObj.SetTask(new TasksController.TaskObj(){Description = "Загляни на кухню", Title = "Нужно покушать"}, 0);
        yield return new WaitForSeconds(2);
        yield return new WaitUntil(() =>chips.isUsed);
        yield return TasksControllerObj.CompleteTask(0);
        yield return new WaitForSeconds(5);
       
        TasksControllerObj.SetTask(new TasksController.TaskObj(){Description = "Прими лекартсва", Title = "Таблетки"}, 0);
        yield return new WaitForSeconds(2);
        yield return new WaitUntil(() =>pills.isUsed);
        yield return TasksControllerObj.CompleteTask(0);
        yield return new WaitForSeconds(5);
        
        PcBlock.SetActive(false);
        
        TasksControllerObj.SetTask(new TasksController.TaskObj(){Description = "Нужно приступать к обучению", Title = "Включить компьютер"}, 0);
        yield return new WaitForSeconds(2);
        yield return new WaitUntil(() =>PcControllerObj.IsPcOn);
        yield return TasksControllerObj.CompleteTask(0);
        
        yield return new WaitForSeconds(2);
        
        TasksControllerObj.SetTask(new TasksController.TaskObj(){Description = "Приступай к обучению", Title = "Заёди в Жтелеком"}, 0);
        yield return new WaitForSeconds(5);
        yield return new WaitUntil(() =>ContractManagerObj.IsStart);
        yield return TasksControllerObj.CompleteTask(0);
        
        
        PlayerObj.FreezingPlayer(true);
        yield return new WaitUntil(() =>ContractManagerObj.Iscompleted);
        PlayerObj.FreezingPlayer(false);


        KnockAudioStart();

        doorOnHome.IsEndoFGame = true;
        
        
        //стук в двер
        //финал
    }
}
