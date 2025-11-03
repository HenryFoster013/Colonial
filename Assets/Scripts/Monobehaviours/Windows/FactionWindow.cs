using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static GenericUtils;

public class FactionWindow : Window {

    [Header("References")]
    [SerializeField] PlayerManager _PlayerManager;
    [SerializeField] GameplayManager _GameplayManager;

    [Header("UI")]
    [SerializeField] Image[] Colorised;
    [SerializeField] Image Flag;
    [SerializeField] TMP_Text Header;
    [SerializeField] TMP_Text NationName;
    [SerializeField] TMP_Text PeaceWarText;
    [SerializeField] GameObject DarkenerEnabled;
    [SerializeField] GameObject DarkenerDisabled;

    Faction faction;

    public void Load(Faction _faction){
        faction =_faction;
        RefreshUI();
        Open();
    }

    public void RefreshUI(){
        if(faction == null)
            return;

        Flag.sprite = faction.Flag();
        foreach(Image colorised in Colorised)
            colorised.color = faction.Colour();
        Header.text = faction.Name();
        NationName.text = faction.Name();
        PeaceWarText.text = "Offer Peace";
        if(_PlayerManager.AtPeace(faction))
            PeaceWarText.text = "Break Peace";
        bool active_func = _GameplayManager.CanUseFactionUI(faction);
        DarkenerEnabled.SetActive(active_func);
        DarkenerDisabled.SetActive(!active_func);
    }

    public bool CheckHarassed(){
        if(!_GameplayManager.CanUseFactionUI(faction)){
            PlaySFX("UI_4", SFX_Lookup);
            return true;
        }
        return false;
    }

    public void Embassy(){
        if(CheckHarassed())
            return;
        Close();
    }

    public void PeaceWar(){
        if(CheckHarassed())
            return;
        _GameplayManager.FlipPeace(faction);
        PlaySFX("Morse_Code", SFX_Lookup);
        SilentClose();
    }

    public void Message(){
        if(CheckHarassed())
            return;
        Close();
    }
}
