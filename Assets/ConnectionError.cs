using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConnectionError : MonoBehaviour
{
    [SerializeField] TMP_Text ErrorInfo;

    void Start(){
        ErrorInfo.text = PlayerPrefs.GetString("Error Details");
    }
}
