using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static HenrysUtils;

public class GameplayManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] TroopLookup _TroopLookup;
    [SerializeField] PlayerManager _PlayerManager;
    [SerializeField] SessionManager _SessionManager;
    [SerializeField] SoundEffectLookup SFX_Lookup;
    ConnectionManager _ConnectionManager;
    MapManager _MapManager;

    [Header("Visuals")]
    [SerializeField] GameObject DamageEffect;
    [SerializeField] GameObject DefeatEffect;
 
    [Networked] public int current_turn { get; private set; }
    public int player_sub_turn;
    int player_count;

    public int current_stars { get; private set; }
    int stars_per_turn = 3;
    int troop_counter = 1;

    public List<Troop> AllTroops;

    // Defaults

    public void Setup(){
        current_stars = 100;
        current_turn = 1;
        player_sub_turn = 0;
        player_count = _SessionManager.player_instances.Count;
        if(_SessionManager.Hosting)
            RPC_SetTurn(0);
        else
            PlaySFX("Drums_2", SFX_Lookup);
        
        ResetTroops();
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
            _PlayerManager.Deselect();
            _PlayerManager.EnableAllTroops();
            PlaySFX("Drums_1", SFX_Lookup);
        }
        else{
            _PlayerManager.DisableAllTroops();
        }

        if(_SessionManager.player_instances[player] != _SessionManager.OurInstance)
            _PlayerManager.UpdateTurnNameDisplay(_SessionManager.player_instances[player].GetUsername() + "'s turn.");
        else
            _PlayerManager.UpdateTurnNameDisplay("Our turn.");
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
            if(AllTroops[i] != null){
                if(AllTroops[i].UniqueID == id) //mark
                    troop = AllTroops[i];
            }
        }
        return troop;
    }

    public void CleanAllTroops(){
        AllTroops.RemoveAll(item => item == null);
    }

    public List<int> WalkableTileFilter(List<int> tiles){
        foreach(Troop t in AllTroops){
            if(t != null)
                tiles.Remove(t.current_tile);
        }
        return tiles;
    }

    public List<int> EnemyTileFilter(List<int> tiles){
        List<int> result = new List<int>();
        foreach(Troop t in AllTroops){
            if(t != null){
                if(tiles.Contains(t.current_tile) && _MapManager.CheckVisibility(t.current_tile) && t.FactionID() != _SessionManager.OurInstance.Faction_ID){
                    result.Add(t.current_tile);
                }
            }
        }
        return result;
    }

    public List<int> SpecialTileFilter(List<int> tiles){
        List<int> result = new List<int>();
        return result;
    }

    // Troop Combat //

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_AttackTroop(int attacking_id, int target_id, bool original, RpcInfo info = default){
        AttackTroop(attacking_id, target_id, original);
    }

    void AttackTroop(int attacking_id, int target_id, bool original_attack){

        Troop attacking_troop = GetTroop(attacking_id);
        Troop target_troop = GetTroop(target_id); // mark
        
        attacking_troop.AttackAnim();
        attacking_troop.RotateAt(target_troop.transform.position);
        if(target_troop != null){
            target_troop.DamageAnim();
            target_troop.RotateAt(attacking_troop.transform.position);
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
        g.transform.position = _MapManager.GetTilePosition(tile);
    }

    // Troop Spawning //

    public Troop GetTroopAt(int tile){
        Troop troop = null;
        foreach(Troop t in AllTroops){
            if(t != null){
                if(t.current_tile == tile)
                    troop = t;
            }
        }
        return troop;
    }

    public void AddTroop(Troop troop){AllTroops.Add(troop);}

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

        CleanAllTroops();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetTroopPos(int troop_id, int tile, int owner){
        Troop troop = GetTroop(troop_id);
        if(_SessionManager.Hosting){
            SetTroopPos(troop, tile, owner);
        }
    }

    public void SetTroopPos(Troop troop, int id, int owner){
        if(troop.Owner == owner && troop != null)
            troop.current_tile = id;
    }

    public void SpendStars(int cost){current_stars -= cost;}
    public void SetConnection(ConnectionManager cm){_ConnectionManager = cm;}
    public void SetMap(MapManager mm){_MapManager = mm;}
}
