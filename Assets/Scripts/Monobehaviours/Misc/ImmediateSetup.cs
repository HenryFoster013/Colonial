using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ImmediateSetup : MonoBehaviour{
    
    [SerializeField] string TitleScreen;
    [SerializeField] string AccountCreation;

    void Start(){
        if(PlayerPrefs.GetString("USERNAME") == "")
            SceneManager.LoadScene(AccountCreation);
        else
            SceneManager.LoadScene(TitleScreen);
    }
}
