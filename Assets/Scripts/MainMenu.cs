using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string QuickplayScene = "Auto Matchmake";
    [SerializeField] string ServerBrowserScene = "Server Browser";
    
    public void QuickPlay(){
        SceneManager.LoadScene(QuickplayScene);
    }

    public void ServerBrowser(){
        SceneManager.LoadScene(ServerBrowserScene);
    }

    public void Exit(){
        Application.Quit();
    }
}
