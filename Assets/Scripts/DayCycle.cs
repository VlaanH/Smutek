using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayCycle : MonoBehaviour
{

   public float TimeOfDay;
   public float DayDuration = 10;

   public DateTime TimeOfDay24 ;
   
   public float days = 1;

   private float nextTime = 0f;
   private bool isNextTime = false;
   private const float Hours24InMinutes = 1440;

   public Gradient dayCycleColor;
   public AnimationCurve intensity;
   public AnimationCurve sunCycle;
   
   public Light Sun;
   
   public Text ClockObj;
   public Material SmokeMaterial;

   public OutsideController OutsideController;



   public void SkipDay()
   {
      SkipTime(0.15f);
   }

   public float GetTimeSpan()
   {
      return Hours24InMinutes*TimeOfDay;
   }
   
   public void SetTime(float minuteTime)
   {
      //1440 = 24*60
      //1H = 60m;
      
      var data =  minuteTime/Hours24InMinutes;
    
      var timeTimeSpan = TimeSpan.FromMinutes(minuteTime);
      SetTimeSpan(timeTimeSpan);
   }

   public void SetTimeSpan(TimeSpan timeTimeSpan)
   {
      TimeOfDay =  (float)(timeTimeSpan.TotalMinutes/Hours24InMinutes);

     
      var intensityValue = intensity.Evaluate(TimeOfDay);
      
      Debug.Log((float)(timeTimeSpan.TotalMinutes/Hours24InMinutes));
      
      TimeOfDay24 = new DateTime(2023, 2,Convert.ToInt32(Math.Floor(days)),timeTimeSpan.Hours,timeTimeSpan.Minutes,timeTimeSpan.Seconds);

      Sun.transform.localRotation = Quaternion.Euler((sunCycle.Evaluate(TimeOfDay) *300)-50,0,0);
         
      ResetTimeOfDay();
      
      
      RenderSettings.fogColor = dayCycleColor.Evaluate(TimeOfDay);

      var color= dayCycleColor.Evaluate(TimeOfDay);
      
      RenderSettings.ambientIntensity = intensityValue+0.36f;
      Sun.intensity = intensityValue*15;
      
      Debug.Log(intensityValue);
      
      var smokeMaterialAIndex = new Color()
      {
         r = color.r,
         g = color.g,
         b = color.b,
         a = 0.15f,
      };
         
      SunSwitch();
      
      SmokeMaterial.color = smokeMaterialAIndex;
         
      ClockObj.text = TimeOfDay24.ToShortTimeString();
   }

   private void SkipTime(float time)
   {
      nextTime = time;
      isNextTime = true;
   }

   private void ResetTimeOfDay()
   {
      if (TimeOfDay >= 1)
      {
         TimeOfDay -= 1;
      }
   }

   private bool IsNight = true;
   private void SunSwitch()
   {
     
      
      if (TimeOfDay24.Hour>18 || TimeOfDay24.Hour<9)
      {
         if (IsNight == false)
         {
            OutsideController.OnStreetLights();
            //Sun.gameObject.SetActive(false);
            IsNight = true;
         }
      }
      else
      {
         if (IsNight==true)
         {
            IsNight = false;
            OutsideController.OffStreetLights();
            //Sun.gameObject.SetActive(true);
         }
        
      }
      
   }


   private float _timePassed = 0;

   private void Start()
   {
      if (TimeOfDay24==default)
      {
         SetTime(530);
      }
      
   }

   private void Update()
   {
      if (isNextTime)
      {
         
         var time = Time.deltaTime / DayDuration;
         _timePassed += time;
         
         days += time;
         
         
         if (_timePassed>nextTime)
         {
            isNextTime = false;
            _timePassed = 0;
         }
        
         
         TimeOfDay += time;
         ResetTimeOfDay();
         
         SetTime(Hours24InMinutes*TimeOfDay);

      }
      
   }
}
