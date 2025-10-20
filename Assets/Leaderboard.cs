using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HenrysMapUtils;

public class Leaderboard : Window{

    [Header("References")]
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] GameplayManager _GameplayManager;
    [SerializeField] MapManager _MapManager;

    const int fort_multiplier = 500;
    const int troop_multiplier = 10;

    Dictionary<Faction, int> fort_count = new Dictionary<Faction, int>();
    Dictionary<Faction, int> troop_count = new Dictionary<Faction, int>();

    // Opening

    public new void SilentOpen(){
        RefreshUI();
        RandomisePosition();
        dragRectTransform.gameObject.SetActive(true);
    }

    public void RefreshUI(){
        foreach(Faction faction in _FactionLookup.GetFactions()){
            int score = CalculateScore(faction);
            if(score > 0){
                // display
            }
        }
    }

    int CalculateScore(Faction fact){
        int score = 0;
        if(troop_count.ContainsKey(fact))
            score += troop_count[fact] * troop_multiplier;
        if(fort_count.ContainsKey(fact))
            score += fort_count[fact] * fort_multiplier;
        return score;
    }

    // Setting

    public void UpdateTroops(){
        Faction faction = null;
        troop_count = new Dictionary<Faction, int>();

        foreach(Troop troop in _GameplayManager.AllTroops){
            faction = troop.FactionData();
            if(troop_count.ContainsKey(faction))
                troop_count[faction]++;
            else
                troop_count.Add(faction, 1);
        }
    }

    public void UpdateFortresses(){
        fort_count = new Dictionary<Faction, int>();

        foreach(Tile fort in _MapManager.GetCities()){
            if(fort.stats != null){
                if(fort_count.ContainsKey(fort.owner))
                    fort_count[fort.owner]++;
                else
                    fort_count.Add(fort.owner, 1);
            }
        }
    }
}