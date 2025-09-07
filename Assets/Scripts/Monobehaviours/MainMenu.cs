using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static HenrysUtils;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string QuickplayScene = "Auto Matchmake";
    [SerializeField] string ServerBrowserScene = "Server Browser";
    [SerializeField] SoundEffectLookup SFX_Lookup;
    
    public void QuickPlay(){
        PlaySFX("UI_2", SFX_Lookup);
        SceneManager.LoadScene(QuickplayScene);
    }

    public void ServerBrowser(){
        PlaySFX("UI_2", SFX_Lookup);
        SceneManager.LoadScene(ServerBrowserScene);
    }

    public void Exit(){
        Application.Quit();
    }
}
