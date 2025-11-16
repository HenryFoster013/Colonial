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
    [SerializeField] Window Settings;
    [SerializeField] Window Credits;
    
    public void QuickPlay(){
        PlayerPrefs.SetString("LOAD ORIGIN", "Title Screen");
        PlaySFX("UI_2", SFX_Lookup);
        SceneManager.LoadScene(QuickplayScene);
        BG.Save();
    }

    public void ServerBrowser(){
        PlayerPrefs.SetString("LOAD ORIGIN", ServerBrowserScene);
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

    public void CreditsMenu(){
        Credits.Open();
    }
}
