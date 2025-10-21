using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FactionLeaderboardTab : MonoBehaviour
{
    [SerializeField] TMP_Text TextDisplay;
    [SerializeField] Image FlagDisplay;
    
    public void UpdateInfo(Faction faction, float ownership_percentage){
        FlagDisplay.sprite = faction.Flag();
        string percentage_text = ((int)Mathf.Round(ownership_percentage)).ToString();
        TextDisplay.text = faction.Name() + " " + percentage_text + "%";
    }
}
