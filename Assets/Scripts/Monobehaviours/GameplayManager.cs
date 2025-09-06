using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GameplayManager : NetworkBehaviour
{
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] TroopLookup _TroopLookup;
    [SerializeField] PlayerManager _PlayerManager;
    SessionManager _SessionManager;
    ConnectionManager _ConnectionManager;
    MapManager _MapManager;

    public int current_turn { get; private set; }
    public int current_stars { get; private set; }
    int stars_per_turn = 3;
    int troop_counter = 1;

    public List<Troop> AllTroops;

    // Defaults

    public void DefaultValues(){
        current_stars = 3;
        current_turn = 1;
        ResetTroops();
    }

    // TROOPS //

    void ResetTroops(){
        AllTroops = new List<Troop>();   
    }

    public List<Troop> GetTroopList(){
        return AllTroops;
    }

    public Troop GetTroop(int id){
        Troop troop = null;
        for(int i = 0; i < AllTroops.Count && troop == null; i++){
            if(AllTroops[i].UniqueID == id)
                troop = AllTroops[i];
        }
        return troop;
    }

    public List<int> WalkableTileFilter(List<int> tiles){
        foreach(Troop t in AllTroops){
            tiles.Remove(t.current_tile);
        }
        return tiles;
    }

    // Troop Spawning //

    public void AddTroop(Troop troop){
        AllTroops.Add(troop);
    }

    public void AskToSpawnTroop(TroopData troop_data, int tile, int owner){
        if(_SessionManager.Hosting){
            SpawnTroop(troop_data, tile, owner);
        }
        else{
            RPC_SpawnTroop(_TroopLookup.ID(troop_data), tile, owner);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SpawnTroop(int troop_id, int tile, int owner, RpcInfo info = default){
        SpawnTroop(_TroopLookup.Troop(troop_id), tile, owner);
    }

    public void SpawnTroop(TroopData troop_data, int tile, int owner){

        if(!_SessionManager.Hosting)
            return;

        if(!_MapManager.CheckTileOwnership(tile, _SessionManager.PlayerFactionID(owner)))
            return;

        NetworkObject new_troop = _ConnectionManager.SpawnObject(troop_data.NetPrefabRef());
        Troop troop = new_troop.gameObject.GetComponent<Troop>();
        troop.Owner = owner;
        troop.Faction_ID = _SessionManager.PlayerFactionID(owner);
        troop.UniqueID = troop_counter;
        troop_counter++;
        troop.current_tile = tile;
    }

    public void AskToMoveTroop(Troop troop, int tile, int owner){
        if(_SessionManager.Hosting){
            SetTroopPos(troop, tile, owner);
        }
        else{
            RPC_SetTroopPos(troop.UniqueID, tile, owner);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetTroopPos(int troop_id, int tile, int owner){
        Troop troop = GetTroop(troop_id);
        SetTroopPos(troop, tile, owner);
    }

    public void SetTroopPos(Troop troop, int id, int owner){
        if(troop.Owner == owner && troop != null)
            troop.current_tile = id;
    }

    public void UpTurn(){current_turn++;}
    public void UpStars(){current_stars += stars_per_turn;}
    public void SpendStars(int cost){current_stars -= cost;}
    public void SetSession(SessionManager sm){_SessionManager = sm;}
    public void SetConnection(ConnectionManager cm){_ConnectionManager = cm;}
    public void SetMap(MapManager mm){_MapManager = mm;}
}
