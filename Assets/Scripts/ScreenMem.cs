using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScreenMem : MonoBehaviour
{
   public static Texture2D LastScreen;


   public static void TakeScreenshot(Camera screenshotCamera)
   {
       RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);


       screenshotCamera.targetTexture = renderTexture;
       screenshotCamera.Render();

       RenderTexture.active = renderTexture;

       Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
       screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
       screenshot.Apply();
       
       LastScreen = screenshot;

       //Destroy(screenshot);
       screenshotCamera.targetTexture = null;
       RenderTexture.active = null;
   }

   public static void SaveScreenshotForSave(string id)
   {
       byte[] bytes = LastScreen.EncodeToPNG();
       string screenshotName = id + ".png";
       System.IO.File.WriteAllBytes(   Application.persistentDataPath + "/" + screenshotName, bytes);

   }

   public static string GetScreenshotForId(string id)
   {
       string screenshotName = id + ".png";
       var patch= Application.persistentDataPath + "/" + screenshotName;
       if (File.Exists(patch))
       {
           return patch;
       }
       else
       {
           return null;
       }
   }


   public static Sprite SpriteFromTexture(Texture2D texture)
   {
       return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
   }

   public static Texture2D LoadTextureFromFile(string path)
   {
       byte[] data = System.IO.File.ReadAllBytes(path);
       Texture2D texture = new Texture2D(2, 2); // Создаем временную текстуру
       if (texture.LoadImage(data))
       {
           return texture;
       }
       else
       {
           return null;
       }
   }
}
