using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static GenericUtils;

public class MessageInputWindow : Window{

    [Header("Message Input Specifics")]
    [SerializeField] TMP_InputField Field; 
    [SerializeField] SoundEffect SendSFX;
    [SerializeField] GameplayManager _GameplayManager;
    Faction faction;

    public void Setup(Faction target){
        Field.text = "";
        faction = target;
        Open();
    }

    public void Send(){
        if(ValidateMessage(Field.text, 120)){
            _GameplayManager.SendMessage(faction, Field.text);
        }
        PlaySFX(SendSFX);
        SilentClose();
    }

}
