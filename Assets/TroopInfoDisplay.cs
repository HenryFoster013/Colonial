using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HenrysMapUtils;

public class TroopInfoDisplay : MonoBehaviour{
    [SerializeField] Image Background;
    [SerializeField] TMP_Text Title;
    [SerializeField] TMP_Text Cost;
    [SerializeField] GameObject[] Ticks;
    [SerializeField] GameObject[] Crosses;
    [SerializeField] StatBar[] Stats;

    const int max_troop_usage = 5;

    public void Refresh(TroopData troop, TileStats spawn_tile, Color bg_color, int money){
        if(troop == null || spawn_tile == null)
            return;

        Title.text = troop.Name();
        Cost.text = troop.Cost().ToString();
        Background.color = bg_color;

        Ticks[0].SetActive(spawn_tile.FreePopulation() >= troop.PopulationCost());
        Crosses[1].SetActive(spawn_tile.FreePopulation() < troop.PopulationCost());
        Ticks[1].SetActive(spawn_tile.FreeProduce() >= troop.ProduceCost());
        Crosses[1].SetActive(spawn_tile.FreeProduce() < troop.ProduceCost());
        Ticks[1].SetActive(spawn_tile.FreeIndustry() >= troop.IndustryCost());
        Crosses[1].SetActive(spawn_tile.FreeIndustry() < troop.IndustryCost());
        Ticks[1].SetActive(money >= troop.Cost());
        Crosses[1].SetActive(money < troop.Cost());

        Stats[0].Refresh(troop.PopulationCost(), 5);
        Stats[1].Refresh(troop.ProduceCost(), 5);
        Stats[2].Refresh(troop.IndustryCost(), 5);
    }
}
