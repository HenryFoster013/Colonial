using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionChecker : MonoBehaviour{

    [SerializeField] GameObject Reference;
    bool Game_Over = false;

    void Update(){
        CheckConnection();
    }

    void CheckConnection(){
        if(Reference == null){
            if(!Game_Over)
                SceneManager.LoadScene("Network Error");
        }
    }

    public void SetOver(bool input){
        Game_Over = input;
    }
}
