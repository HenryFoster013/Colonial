using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] PlayerManager _PlayerManager;
    SessionManager _SessionManager;
    ConnectionManager _ConnectionManager;
    MapManager _MapManager;

    public int current_turn { get; private set; }
    public int current_stars { get; private set; }
    int stars_per_turn = 3;

    List<Troop>[] AllTroops;

    public void DefaultValues(){
        current_stars = 3;
        current_turn = 1;
        ResetTroops();
    }

    // TROOPS //

    void ResetTroops(){
        AllTroops = new List<Troop>[5];
        for(int i = 0; i < AllTroops.Length; i++)
            AllTroops[i] = new List<Troop>();
    }

    public List<Troop> GetTroops(int id){
        return AllTroops[id];
    }

    public void SpawnTroop(TroopData troop_data, int tile, int owner){

        if(!_SessionManager.Hosting)
            return;

        if(!_MapManager.CheckTileOwnership(tile, _SessionManager.PlayerFactionID(owner)))
            return;

        NetworkObject new_troop = _ConnectionManager.SpawnObject(troop_data.NetPrefabRef());
        Troop troop = new_troop.gameObject.GetComponent<Troop>();
        troop.InitialSetup(_SessionManager, _MapManager, _FactionLookup, _SessionManager.PlayerFaction(owner), owner);
        //troop.SetPosition(tile);
    }

    public void MoveTroop(Troop troop, int id){
        //troop.SetPosition(id);
    }

    public void UpTurn(){current_turn++;}
    public void UpStars(){current_stars += stars_per_turn;}
    public void SpendStars(int cost){current_stars -= cost;}
    public void SetSession(SessionManager sm){_SessionManager = sm;}
    public void SetConnection(ConnectionManager cm){_ConnectionManager = cm;}
    public void SetMap(MapManager mm){_MapManager = mm;}
}
