using UnityEngine;
using System.IO;
using System.Collections;

public class DebugRenderer : MonoBehaviour{

    [SerializeField] Camera Cam;
    [SerializeField] RenderTexture Render;
    [SerializeField] string Path;

    void Update(){
        if (Input.GetKeyDown(KeyCode.Space))
            Screenshot();
    }

    void Screenshot(){
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        string fileName = $"Screenshot_{timestamp}.png";

        Cam.Render();
        Texture2D screenshot = new Texture2D(Render.width, Render.height, TextureFormat.ARGB32, false);
        RenderTexture.active = Render;
        screenshot.ReadPixels(new Rect(0, 0, Render.width, Render.height), 0, 0);
        screenshot.Apply();

        Color[] pixels = screenshot.GetPixels();
        for (int i = 0; i < pixels.Length; i++){
            pixels[i] = pixels[i].gamma;
        }
        screenshot.SetPixels(pixels);
        screenshot.Apply();

        byte[] bytes = screenshot.EncodeToPNG();
        Destroy(screenshot);

        string filePath = Path + "\\" + fileName;
        File.WriteAllBytes(filePath, bytes);

        Debug.Log("Screenshot taken, " + timestamp);
    }
}