using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HenrysMapUtils;

public class SpawnInfoDisplay : MonoBehaviour{
    [SerializeField] Image Background;
    [SerializeField] TMP_Text Title;
    [SerializeField] TMP_Text Cost;
    [SerializeField] GameObject[] Ticks;
    [SerializeField] GameObject[] Crosses;
    [SerializeField] StatBar[] Stats;

    const int max_troop_usage = 5;
    TroopData last_troop;
    TileStats last_tile;

    public void Refresh(TroopData troop, TileStats spawn_tile, Faction faction, int money){
        if(troop == null || spawn_tile == null)
            return;
        
        TicksCrosses(money, troop, spawn_tile);

        if(troop == last_troop && spawn_tile == last_tile)
            return;

        last_troop = troop;
        last_tile = spawn_tile;

        Title.text = troop.Name();
        Cost.text = faction.Currency() + troop.Cost().ToString();
        Background.color = faction.Colour();

        Stats[0].Refresh(5, troop.PopulationCost());
        Stats[1].Refresh(5, troop.ProduceCost());
        Stats[2].Refresh(5, troop.IndustryCost());
    }

    void TicksCrosses(int money, TroopData troop, TileStats spawn_tile){
        bool can_afford_pop = spawn_tile.FreePopulation() >= troop.PopulationCost();
        bool can_afford_prod = spawn_tile.FreeProduce() >= troop.ProduceCost();
        bool can_afford_ind = spawn_tile.FreeIndustry() >= troop.IndustryCost();
        bool can_afford_curr = money >= troop.Cost();

        Ticks[0].SetActive(can_afford_pop);
        Crosses[0].SetActive(!can_afford_pop);
        Ticks[1].SetActive(can_afford_prod);
        Crosses[1].SetActive(!can_afford_prod);
        Ticks[2].SetActive(can_afford_ind);
        Crosses[2].SetActive(!can_afford_ind);
        Ticks[3].SetActive(can_afford_curr);
        Crosses[3].SetActive(!can_afford_curr);
    }
}
