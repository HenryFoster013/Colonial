using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FactionLeaderboardTab : MonoBehaviour
{
    [SerializeField] TMP_Text TextDisplay;
    [SerializeField] Image FlagDisplay;
    [SerializeField] GameObject WarIcon;
    [SerializeField] GameObject PeaceIcon;
    Faction faction;
    PlayerCommunicationManager player;
    
    public void UpdateInfo(Faction _faction, float ownership_percentage, PlayerCommunicationManager pcm){
        
        player = pcm;
        faction = _faction;
        
        bool peaceful = player.AtPeace(faction);
        bool are_we = player.AreWe(faction);
        WarIcon.SetActive(!peaceful && !are_we);
        PeaceIcon.SetActive(peaceful && !are_we);
        
        
        FlagDisplay.sprite = faction.Flag();
        string percentage_text = ((int)Mathf.Round(ownership_percentage)).ToString();
        TextDisplay.text = faction.Name() + " " + percentage_text + "%";
    }

    public void Pressed(){
        player.OpenFactionInformation(faction);
    }
}
