using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FactionWindow : Window {

    [Header("UI")]
    [SerializeField] Image[] Colorised;
    [SerializeField] Image Flag;
    [SerializeField] TMP_Text Header;
    [SerializeField] TMP_Text NationName;
    [SerializeField] TMP_Text PeaceWarText;

    Faction faction;
    PlayerManager player;

    public void Load(Faction _faction, PlayerManager playa){
        faction  =_faction;
        player = playa;

        Flag.sprite = faction.Flag();
        foreach(Image colorised in Colorised){
            colorised.color = faction.Colour();
        }

        Header.text = faction.Name();
        NationName.text = faction.Name();
        PeaceWarText.text = "Offer Peace";
        if(player.AtPeace(faction))
            PeaceWarText.text = "Break Peace";

        Open();
    }

    public void Embassy(){

        Close();
    }

    public void PeaceWar(){

        Close();
    }

    public void Message(){

        Close();
    }
}
