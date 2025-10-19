using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HenrysMapUtils;

public class SpawnInfoDisplay : MonoBehaviour{
    [SerializeField] GameObject FullBackground;
    [SerializeField] GameObject ShortenedBackground;
    [SerializeField] TMP_Text Title;
    [SerializeField] TMP_Text Cost;
    [SerializeField] GameObject[] Ticks;
    [SerializeField] GameObject[] Crosses;
    [SerializeField] StatBar[] Stats;

    const int max_troop_usage = 5;
    TroopData last_troop;
    TileStats last_tile;
    PieceData last_piece;

    public void Refresh(TroopData troop, TileStats spawn_tile, Faction faction, int money){
        if(troop == null || spawn_tile == null)
            return;

        TicksCrosses(money, troop, spawn_tile);
        last_piece = null;

        if(troop == last_troop && spawn_tile == last_tile)
            return;

        last_troop = troop;
        last_tile = spawn_tile;

        Title.text = troop.Name();
        Cost.text = faction.CurrencyFormat(troop.Cost());
        FullBackground.SetActive(true);
        ShortenedBackground.SetActive(false);

        Stats[0].Refresh(5, troop.PopulationCost());
        Stats[1].Refresh(5, troop.ProduceCost());
        Stats[2].Refresh(5, troop.IndustryCost());
    }

    public void Refresh(PieceData piece, int money, Faction faction){
        if(piece == null)
            return;
        
        last_troop = null;
        last_tile = null;

        foreach(GameObject g in Ticks)
            g.SetActive(false);
        foreach(GameObject g in Crosses)
            g.SetActive(false);

        Ticks[3].SetActive(money >= piece.Cost());
        Crosses[3].SetActive(piece.Cost() > money);

        if(last_piece == piece)
            return;

        Title.text = piece.Name();
        Cost.text = faction.CurrencyFormat(piece.Cost());
        FullBackground.SetActive(false);
        ShortenedBackground.SetActive(true);
        
        Stats[0].Refresh(5, piece.Population());
        Stats[1].Refresh(5, piece.Produce());
        Stats[2].Refresh(5, piece.Industry());

        last_piece = piece;
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
