using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuConroller : MonoBehaviour
{
   public Image ProgressBar;
   public Text ProgressText;

   public static string scenId;
   private static MainMenuConroller instance;

   public List<GameObject> MenuPages;
   public BackgroundMusic BackgroundMusic;

   private void Start()
   {
       
       instance = this;
       BackgroundMusic.PlayMusic();
   }

   public enum EPagesMenu
   {
       MainMenu,
       Saves,
       LoadScreen

   }

   public void HidPage(int id)
   {
       MenuPages[id].SetActive(false);
   }

   public void GoToMenuPage(int id)
   {
       foreach (var menuPage in MenuPages)
       {
           menuPage.SetActive(false);
       }
       
       MenuPages[(int)id].SetActive(true);
   }
   
   
   
   public static void NewGame()
   {
       
       instance.GoToMenuPage((int)EPagesMenu.LoadScreen);
       instance.StartCoroutine( LoadYourAsyncScene() );

   }

   public static void LoadSaveAndScene(string id )
   {
       scenId = id;
       instance.GoToMenuPage((int)EPagesMenu.LoadScreen);
       
       instance.StartCoroutine( LoadYourAsyncScene());
       
   }

   public static void LoadMainMenuScene()
   {
       instance.StartCoroutine( LoadYourAsyncScene("Scenes/MainMenu"));
   }


   public static IEnumerator LoadYourAsyncScene(string scene = "Scenes/SampleScene") 
   {
      
       SceneManager.sceneLoaded +=  LoadSave;
       AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);
       float oldProgress = 0;

       do
       {
           var valueLerp = Mathf.Lerp(oldProgress, asyncLoad.progress, Time.deltaTime * 5);

           instance.ProgressBar.fillAmount = valueLerp;
           instance.ProgressText.text = "ЗАГРУЗКА -" + Mathf.RoundToInt(valueLerp * 100) + "%";

           oldProgress = asyncLoad.progress;

           if (Mathf.RoundToInt(valueLerp * 100) == 90)
           {
               instance.ProgressText.text = "ЗАГРУЗКА -" + 100 + "%";
           }

           yield return null;

       } while (!asyncLoad.isDone);
      
       
       yield return new WaitForSeconds(2);

   }

   

   private static void LoadSave(Scene cene, LoadSceneMode arg1)
   {
       if (scenId!=default)
       {
           if (cene.name == "SampleScene")
           {
               SavesManager.LoadGame(scenId);
           }
       }
       
      
   }
}
