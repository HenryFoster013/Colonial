using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static GenericUtils;
using MapUtils;
using EventUtils;

public class GameplayManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] TroopLookup _TroopLookup;
    [SerializeField] PieceLookup _PieceLookup;
    [SerializeField] PlayerManager _PlayerManager;
    [SerializeField] SessionManager _SessionManager;
    [SerializeField] SoundEffectLookup SFX_Lookup;
    ConnectionManager _ConnectionManager;
    [SerializeField] MapManager _MapManager;
    [SerializeField] MessageManager _MessageManager;

    [Header("Visuals")]
    [SerializeField] GameObject DamageEffect;
    [SerializeField] GameObject DefeatEffect;
 
    [Networked] public int current_turn { get; private set; }
    public int player_sub_turn;
    int player_count;
    public bool our_first_turn;

    int troop_counter = 1;
    public List<Troop> AllTroops;

    // Defaults

    public void Setup(){
        our_first_turn = true;
        current_turn = 1;
        player_sub_turn = 0;
        MessageManager.Setup();
        _SessionManager.SetMoney(5);
        player_count = _SessionManager.player_instances.Count;
        if(_SessionManager.Hosting)
            RPC_SetTurn(0);
        else
            PlaySFX("Drums_2", SFX_Lookup);
        
        ResetTroops();
    }

    void Update(){
        RefreshAllCities();
    }

    // TURNS //

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AskToMoveTurn(int id, RpcInfo info = default){
        if(id == _SessionManager.player_instances[player_sub_turn].ID){
            player_sub_turn++;
            if(player_sub_turn >= _SessionManager.player_instances.Count){
                player_sub_turn = 0;
                current_turn++;
            }
            RPC_SetTurn(_SessionManager.player_instances[player_sub_turn].ID);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetTurn(int player, RpcInfo info = default){

        bool our_turn = _SessionManager.OurInstance.ID == player;

        _PlayerManager.OurTurn = our_turn;
        if(our_turn){
            NewTurn();
        }
        else{
            _PlayerManager.DisableAllTroops();
        }

        if(_SessionManager.player_instances[player] != _SessionManager.OurInstance)
            _PlayerManager.UpdateTurnNameDisplay(_SessionManager.player_instances[player].GetUsername() + "'s turn.");
        else
            _PlayerManager.UpdateTurnNameDisplay("Our turn.");

        MessageManager.Tick(current_turn, player);
    }

    void NewTurn(){
        _PlayerManager.Deselect();
        _PlayerManager.EnableAllTroops();

        PlaySFX("Drums_1", SFX_Lookup);

        if(our_first_turn){
            our_first_turn = false;
        }
        else
            _SessionManager.EarnMoney(_MapManager.TotalValue());
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
            if(ValidateTroop(AllTroops[i])){
                if(AllTroops[i].UniqueID == id) //mark
                    troop = AllTroops[i];
            }
        }
        return troop;
    }

    public void CleanAllTroops(){
        AllTroops.RemoveAll(item => item == null);
    }

    public List<Tile> WalkableTileFilter(List<Tile> tiles){
        foreach(Troop t in AllTroops){
            if(ValidateTroop(t))
                tiles.Remove(_MapManager.GetTile(t.current_tile));
        }
        return tiles;
    }

    public List<Tile> EnemyTileFilter(List<Tile> tiles){
        List<Tile> result = new List<Tile>();
        foreach(Troop t in AllTroops){
            if(ValidateTroop(t)){
                Tile tile = _MapManager.GetTile(t.current_tile);
                if(tiles.Contains(tile) && tile.visible && t.FactionID() != _SessionManager.OurInstance.Faction_ID){
                    result.Add(tile);
                }
            }
        }
        return result;
    }

    public List<Tile> SpecialTileFilter(List<Tile> tiles){
        List<Tile> result = new List<Tile>();
        return result;
    }

    // Troop Combat //

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_AttackTroop(int attacking_id, int target_id, bool original, RpcInfo info = default){
        AttackTroop(attacking_id, target_id, original, GetTroop(target_id).transform.position);
    }

    void AttackTroop(int attacking_id, int target_id, bool original_attack, Vector3 old_target_pos){

        Troop attacking_troop = GetTroop(attacking_id);
        Troop target_troop = GetTroop(target_id);

        if(!ValidateTroop(attacking_troop))
            return;
        
        attacking_troop.AttackAnim();
        attacking_troop.RotateAt(old_target_pos);

        if(ValidateTroop(target_troop)){
            target_troop.DamageAnim();
            target_troop.RotateAt(attacking_troop.transform.position);
            attacking_troop.RotateAt(target_troop.transform.position);
        }

        if(attacking_troop.Owner == _SessionManager.OurInstance.ID){
            attacking_troop.UseMove();
            attacking_troop.UseSpecial();
            if(original_attack)
                _PlayerManager.Deselect();
        }

        if(_SessionManager.Hosting){
            target_troop.health -= CalculateDamage(attacking_troop, original_attack);
            if(target_troop.health <= 0){
                if(original_attack && attacking_troop.Data.MoveOnCloseKill()){
                    if(_MapManager.TilesAreNeighbors(attacking_troop.current_tile, target_troop.current_tile))
                        attacking_troop.current_tile = target_troop.current_tile;
                }
                RPC_KilledTroop(target_troop.current_tile);
                _ConnectionManager.Despawn(target_troop.gameObject.GetComponent<NetworkObject>());
            }
            else{
                if(original_attack){
                    StartCoroutine(Slapback(attacking_id, target_id));
                }
                RPC_DamageEffect(target_troop.current_tile);
            }
        }

        CleanAllTroops();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_KilledTroop(int tile, RpcInfo info = default){
        PlaySFX("Drums_4", SFX_Lookup);
        SpawnEffect(DefeatEffect, tile);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_DamageEffect(int tile, RpcInfo info = default){
        SpawnEffect(DamageEffect, tile);
    }

    IEnumerator Slapback(int attacking_id, int target_id){
        yield return new WaitForSeconds(0.5f);
        RPC_AttackTroop(target_id, attacking_id, false);
    }

    int CalculateDamage(Troop troop, bool original){
        int damage = troop.Data.Damage();
        if(!original)
            damage = damage / 2;
        return damage;
    }

    void SpawnEffect(GameObject effect, int tile){
        GameObject g = GameObject.Instantiate(effect);
        g.transform.position = _MapManager.CalcTileWorldPosition(tile);
    }

    // Building Spawning //

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SpawnBuilding(int tile_id, int piece_id, RpcInfo info = default){
        if(!BuildingValid(_MapManager.GetTile(tile_id), _PieceLookup.Piece(piece_id)))
            return;
        _MapManager.RPC_PieceChanged(tile_id, piece_id, true);
    }

    // Troop Spawning //

    public Troop GetTroopAt(Tile tile){
        Troop troop = null;
        foreach(Troop t in AllTroops){
            if(ValidateTroop(t)){
                if(t.current_tile == tile.ID)
                    troop = t;
            }
        }
        return troop;
    }

    public void AddTroop(Troop troop){
        AllTroops.Add(troop);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SpawnTroop(int troop_id, int tile, int owner, RpcInfo info = default){
        SpawnTroop(_TroopLookup.Troop(troop_id), _MapManager.GetTile(tile), owner);
    }

    public bool ValidTroopSpawn(TroopData troop, Tile tile){
        bool valid = IsTileFortress(tile);

        if(valid)
            valid = troop.PopulationCost() <= tile.stats.FreePopulation();
        if(valid)
            valid = troop.ProduceCost() <= tile.stats.FreeProduce();
        if(valid)
            valid = troop.IndustryCost() <= tile.stats.FreeIndustry();

        return valid;
    }

    public void SpawnTroop(TroopData troop_data, Tile tile, int owner){

        if(!_SessionManager.Hosting)
            return;

        if(TroopOnTile(tile))
            return;

        if(!CheckTileOwnership(tile, _FactionLookup.GetFaction(_SessionManager.PlayerFactionID(owner))))
            return;

        if(!ValidTroopSpawn(troop_data, tile))
            return;

        NetworkObject new_troop = _ConnectionManager.SpawnObject(troop_data.NetPrefabRef());
        Troop troop = new_troop.gameObject.GetComponent<Troop>();
        troop.Owner = owner;
        troop.HomeTile = tile.ID;
        troop.Faction_ID = _SessionManager.PlayerFactionID(owner);
        troop.SetName(_FactionLookup.GetFaction(troop.Faction_ID).GetTroopName());
        troop.UniqueID = troop_counter;
        troop_counter++;
        troop.current_tile = tile.ID;

        CleanAllTroops();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetTroopPos(int troop_id, int tile, int owner){
        Troop troop = GetTroop(troop_id);
        if(_SessionManager.Hosting){
            SetTroopPos(troop, tile, owner);
        }
    }

    public bool TroopOnTile(Tile tile){
        bool return_val = false;
        for(int i = 0; i < AllTroops.Count && !return_val; i++){
            if(ValidateTroop(AllTroops[i]))
                return_val = AllTroops[i].current_tile == tile.ID;
        }
        return return_val;
    }

    public void SetTroopPos(Troop troop, int id, int owner){
        if(ValidateTroop(troop)){
            if(troop.Owner == owner)
                troop.current_tile = id;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ConquestNow(int tile, int owner){
        Faction faction = _FactionLookup.GetFaction(owner);
        Tile tile_ = _MapManager.GetTile(tile);
        if(_SessionManager.Hosting){
            if(!_MapManager.IsOwner(tile_, faction)){
                _MapManager.Conquer(tile_, faction);
            }
        }
    }

    void RefreshAllCities(){
        _MapManager.CleanCities();
        foreach(Troop troop in AllTroops){
            if(ValidateTroop(troop))
                _MapManager.AddTroopStats(troop);
        }
    }

    public void SpendMoney(int cost){_SessionManager.SpendMoney(cost);}
    public void SetConnection(ConnectionManager cm){_ConnectionManager = cm;}
}
