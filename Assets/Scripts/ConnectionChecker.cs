using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionChecker : MonoBehaviour{

    [SerializeField] GameObject Reference;

    void Update(){
        CheckConnection();
    }

    void CheckConnection(){
        if(Reference == null)
            SceneManager.LoadScene("Network Error");
    }
}
