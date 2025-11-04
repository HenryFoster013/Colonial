using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static GenericUtils;

public class FactionWindow : Window {

    [Header("References")]
    [SerializeField] PlayerManager _PlayerManager;
    [SerializeField] PlayerCommunicationManager _PlayerCommunicationManager;
    [SerializeField] TechTreeManager _TechTreeManager;
    public MessageInputWindow PrivateMessageWindow;

    [Header("UI")]
    [SerializeField] Image[] Colorised;
    [SerializeField] Image Flag;
    [SerializeField] TMP_Text Header;
    [SerializeField] TMP_Text NationName;
    [SerializeField] TMP_Text PeaceWarText;
    [SerializeField] GameObject DarkenerEnabled;
    [SerializeField] GameObject DarkenerDisabled;

    Faction faction;

    // UI BASE //

    public void Load(Faction _faction){
        faction =_faction;
        RefreshUI();
        Open();
    }


    public void RefreshUI(){
        if(faction == null)
            return;

        Flag.sprite = faction.Flag();
        Header.text = faction.Name();
        NationName.text = faction.Name();

        PeaceWarText.text = "Offer Peace";
        if(_PlayerManager.AtPeace(faction))
            PeaceWarText.text = "Break Peace";

        foreach(Image colorised in Colorised)
            colorised.color = faction.Colour();

        bool active_func = _PlayerCommunicationManager.CanUseFactionUI(faction);
        DarkenerEnabled.SetActive(active_func);
        DarkenerDisabled.SetActive(!active_func);
    }

    // CHECKS //

    public bool CheckHarassed(){
        if(!_PlayerCommunicationManager.CanUseFactionUI(faction)){
            PlaySFX("UI_4", SFX_Lookup);
            return true;
        }
        return false;
    }

    public bool CheckDiplomacy(){
        if(!_TechTreeManager.Unlocked("DIPLOMACY")){
            PlaySFX("UI_4", SFX_Lookup);
            return true;
        }
        return false;    
    }

    // UI BUTTONS //

    public void PrivateMessageButton(){
        if(CheckHarassed() || CheckDiplomacy())
            return;
        PrivateMessageWindow.Setup(faction);
    }

    public void PeaceWar(){
        if(CheckHarassed() || CheckDiplomacy())
            return;
        _PlayerCommunicationManager.FlipPeace(faction);
        PlaySFX("Morse_Code", SFX_Lookup);
        SilentClose();
    }
}
