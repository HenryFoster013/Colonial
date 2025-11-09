using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ImmediateSetup : MonoBehaviour{
    
    [Header("Scenes")]
    [SerializeField] string TitleScreen;
    [SerializeField] string AccountCreation;

    [Header("References")]
    [SerializeField] SettingsManager Settings;
    [SerializeField] QuickQuit Quitter;

    void Start(){
        Quitter.Setup();
        Settings.Setup();
        Exit();
    }

    void Exit(){
        if(PlayerPrefs.GetString("USERNAME") == "")
            SceneManager.LoadScene(AccountCreation);
        else
            SceneManager.LoadScene(TitleScreen);
    }
}
