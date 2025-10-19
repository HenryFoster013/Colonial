using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TroopUIManager : MonoBehaviour
{
    [Header(" - Main - ")]
    public Troop troop;
    [SerializeField] GameObject UI_Holder;
    [SerializeField] Canvas _Canvas;
    [Header(" - Health - ")]
    [SerializeField] Image Health_BG;
    [SerializeField] TMP_Text HealthText;
    [Header(" - Conquest - ")]
    [SerializeField] GameObject Conquer_Holder;

    public void SetCamera(Camera cam){
        _Canvas.worldCamera = cam;
    }

    public void SetHealthColour(Color colour){
        Health_BG.color = colour;
    }

    public void UpdateHealth(){
        HealthText.text = troop.health.ToString();
    }
    
    public void SetVisible(bool visible){
        UI_Holder.SetActive(visible);
    }

    public void ConquerVisible(bool visible){
        Conquer_Holder.SetActive(visible);
    }

    public void ConquerButton(){
        troop.ConquestNow();
    }
}
