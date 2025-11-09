using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GenericUtils;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string QuickplayScene = "Auto Matchmake";
    [SerializeField] string ServerBrowserScene = "Server Browser";
    [SerializeField] SoundEffectLookup SFX_Lookup;
    [SerializeField] BackgroundColouring BG;
    [SerializeField] SettingsWindow Settings;
    
    public void QuickPlay(){
        PlaySFX("UI_2", SFX_Lookup);
        SceneManager.LoadScene(QuickplayScene);
        BG.Save();
    }

    public void ServerBrowser(){
        PlaySFX("UI_2", SFX_Lookup);
        SceneManager.LoadScene(ServerBrowserScene);
        BG.Save();
    }

    public void Exit(){
        Application.Quit();
        BG.Save();
    }

    public void SettingsMenu(){
        Settings.Open();
    }
}
