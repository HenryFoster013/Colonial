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
    PlayerManager player;
    
    public void UpdateInfo(Faction _faction, float ownership_percentage, bool peaceful, bool us, PlayerManager pm){
        WarIcon.SetActive(!peaceful && !us);
        PeaceIcon.SetActive(peaceful && !us);
        faction = _faction;
        player = pm;
        FlagDisplay.sprite = faction.Flag();
        string percentage_text = ((int)Mathf.Round(ownership_percentage)).ToString();
        TextDisplay.text = faction.Name() + " " + percentage_text + "%";
    }

    public void Pressed(){
        player.OpenFactionInformation(faction);
    }
}
