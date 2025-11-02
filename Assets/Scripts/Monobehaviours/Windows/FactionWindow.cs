using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FactionWindow : Window {

    [Header("References")]
    [SerializeField] PlayerManager _PlayerManager;
    [SerializeField] GameplayManager _GamePlayManager;
    
    [Header("UI")]
    [SerializeField] Image[] Colorised;
    [SerializeField] Image Flag;
    [SerializeField] TMP_Text Header;
    [SerializeField] TMP_Text NationName;
    [SerializeField] TMP_Text PeaceWarText;

    Faction faction;

    public void Load(Faction _faction){
        faction =_faction;
        RefreshUI();
        Open();
    }

    public void RefreshUI(){
        Flag.sprite = faction.Flag();
        foreach(Image colorised in Colorised)
            colorised.color = faction.Colour();
        Header.text = faction.Name();
        NationName.text = faction.Name();
        PeaceWarText.text = "Offer Peace";
        if(_PlayerManager.AtPeace(faction))
            PeaceWarText.text = "Break Peace";
    }

    public void Embassy(){
        Close();
    }

    public void PeaceWar(){
        _GamePlayManager.FlipPeace(faction);
        Close();
    }

    public void Message(){
        Close();
    }
}
