using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MapUtils;
using static GenericUtils;
using UnityEngine.UI;

public class Leaderboard : Window{

    [Header("References")]
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] GameplayManager _GameplayManager;
    [SerializeField] PlayerManager _PlayerManager;
    [SerializeField] MapManager _MapManager;
    [Header("Main")]
    [SerializeField] GameObject TabPrefab;
    [SerializeField] Transform TabHook;

    int fort_mult = 10;
    int troop_mult = 1;

    int[] scores;
    int total_score;

    Dictionary<Faction, int> fort_count = new Dictionary<Faction, int>();
    Dictionary<Faction, int> troop_count = new Dictionary<Faction, int>();

    // UI //

    public override void Open(){
        if(CloseOnSecondOpen && dragRectTransform.gameObject.activeSelf)
            Close();
        else{
            PlaySFX("UI_1", SFX_Lookup);
            RefreshUI();
            SilentOpen();
        }
    }

    public void RefreshUI(){
        UpdateTroops();
        UpdateFortresses();
        CalculateAllScores();

        foreach(Transform t in TabHook){
            Destroy(t.gameObject);
        }

        float float_total = (float)total_score;
        int genuine = 0;

        for(int i = 0; i < scores.Length; i++){
            if(scores[i] > 0){

                float percentage = ((float)scores[i] / float_total) * 100f;
                GameObject new_tab = GameObject.Instantiate(TabPrefab);
                Faction faction = _FactionLookup.GetFaction(i);
                int location = 1 + genuine;
                if(_GameplayManager.AreWe(faction))
                    location = 0;
                else
                    genuine++;

                new_tab.transform.SetParent(TabHook);
                new_tab.transform.localPosition = Vector3.zero;
                new_tab.transform.localScale = new Vector3(1,1,1);

                new_tab.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, location * -30f);
                new_tab.GetComponent<FactionLeaderboardTab>().UpdateInfo(faction, percentage, _GameplayManager.AtPeace(faction), _GameplayManager.AreWe(faction), _PlayerManager);
            }
        }
    }

    // Score Calculations //

    void CalculateAllScores(){
        total_score = 0;
        Faction[] factions = _FactionLookup.GetFactions();
        scores = new int[factions.Length];

        for(int i = 0; i < factions.Length; i++){
            int score = CalculateScore(factions[i]);
            scores[i] = score;
            total_score += score;
        }
    }

    int CalculateScore(Faction fact){
        int score = 0;
        if(troop_count.ContainsKey(fact))
            score += troop_count[fact] * troop_mult;
        if(fort_count.ContainsKey(fact))
            score += fort_count[fact] * fort_mult;
        return score;
    }

    // Dictionary Updating //

    public void UpdateTroops(){
        Faction faction = null;
        troop_count = new Dictionary<Faction, int>();

        foreach(Troop troop in _GameplayManager.AllTroops){
            faction = _FactionLookup.GetFaction(troop.Faction_ID);
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