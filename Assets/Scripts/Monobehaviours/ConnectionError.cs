using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static GenericUtils;

public class ConnectionError : MonoBehaviour
{
    [SerializeField] TMP_Text ErrorInfo;
    [SerializeField] SoundEffectLookup SFX_Lookup;

    void Start(){
        PlaySFX("UI_Error", SFX_Lookup);
        ErrorInfo.text = PlayerPrefs.GetString("Error Details");
    }
}
