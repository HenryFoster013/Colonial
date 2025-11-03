using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static GenericUtils;
using MapUtils;
using EventUtils;
using TruceUtils;

public class GameplayManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] TroopLookup _TroopLookup;
    [SerializeField] PieceLookup _PieceLookup;
    [SerializeField] PlayerManager _PlayerManager;
    [SerializeField] SessionManager _SessionManager;
    [SerializeField] SoundEffectLookup SFX_Lookup;
    [HideInInspector] public ConnectionManager _ConnectionManager;
    [SerializeField] MapManager _MapManager;
    [SerializeField] EventManager _EventManager;

    [Header("Visuals")]
    [SerializeField] GameObject DamageEffect;
    [SerializeField] GameObject DefeatEffect;
 
    [Networked] public int current_turn { get; private set; }
    public int player_sub_turn;
    int player_count;
    public bool our_first_turn;

    int troop_counter = 1;
    public List<Troop> AllTroops;

    public TruceManager truce_manager {get; private set;}

    // Defaults

    public void Setup(){
        harassed_factions = new bool[_FactionLookup.Length()];
        harassed_by = new bool[_FactionLookup.Length()];
        our_first_turn = true;
        current_turn = 1;
        player_sub_turn = 0;
        truce_manager = new TruceManager();
        _EventManager.Setup();
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
        if(_PlayerManager.OurTurn && !our_turn) // Our turn just passed
            _EventManager.CleanTurnSensitiveAlerts();
        _PlayerManager.OurTurn = our_turn;

        if(our_turn)
            NewTurn();
        else
            _PlayerManager.DisableAllTroops();

        if(_SessionManager.player_instances[player] != _SessionManager.OurInstance)
            _PlayerManager.UpdateTurnNameDisplay(_SessionManager.player_instances[player].GetUsername() + "'s turn.");
        else
            _PlayerManager.UpdateTurnNameDisplay("Our turn.");
    }

    void NewTurn(){
        _MapManager.RecalculateTotalValue();
        harassed_factions = new bool[_FactionLookup.Length()];
        harassed_by = new bool[_FactionLookup.Length()];
        _PlayerManager.CloseUnnecessaryWindows();
        _PlayerManager.Deselect();
        _PlayerManager.EnableAllTroops();

        PlaySFX("Drums_1", SFX_Lookup);

        if(our_first_turn){
            our_first_turn = false;
        }
        else
            _SessionManager.EarnMoney(_MapManager.TotalValue());
        
        _EventManager.CleanTurnSensitiveAlerts();
        _EventManager.Tick();
    }

    // EVENTS //

    bool[] harassed_factions;
    bool[] harassed_by;

    public string LocalUsername(){return _SessionManager.OurInstance.GetUsername();}

    public bool CanUseFactionUI(Faction target){
        if(!_PlayerManager.OurTurn)
            return false;
        if(Harassed(_FactionLookup.ID(target)))
            return false;
        return true;
    }

    public void FlipPeace(Faction target){
        if(!CanUseFactionUI(target))
            return;

        int target_id =  _FactionLookup.ID(target);
        harassed_factions[target_id] = true;
        if(!AtPeace(target)){
            _PlayerManager.LeaderboardWindow.Close();
            _PlayerManager.DisableAllTroops();
            RPC_OfferTreaty(_SessionManager.OurInstance.Faction_ID, target_id, LocalUsername());
        }
        else
            MakeWar(target);
        
        _PlayerManager.LeaderboardWindow.RefreshUI();
        _PlayerManager.FactionInfoWindow.RefreshUI();
    }

    public void MessageFaction(Faction target){

        if(!_PlayerManager.OurTurn)
            return;

        int target_id =  _FactionLookup.ID(target);
        if(Harassed(target_id))
            return;
        
        // send message event here
    }

    bool Harassed(int target_id){
        if(target_id == _SessionManager.OurInstance.Faction_ID)
            return false;
        return harassed_factions[target_id];
    }

    bool HarassedBy(int target_id){
        if(target_id == _SessionManager.OurInstance.Faction_ID)
            return false;
        return harassed_by[target_id];
    }

    public void SendMessage(Faction faction, string message){
        int faction_id = _FactionLookup.ID(faction);
        if(Harassed(faction_id) || !ValidateMessage(message, 120))
            return;
        harassed_factions[faction_id] = true;
        _PlayerManager.FactionInfoWindow.RefreshUI();
        RPC_SendMessage(faction_id, _FactionLookup.ID(_SessionManager.OurInstance.FactionData()), message, LocalUsername());
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendMessage(int faction_id, int from_id, string message, string username){
        if(_FactionLookup.GetFaction(faction_id) != _SessionManager.OurInstance.FactionData())
            return;
        if(!ValidateMessage(message, 120))
            return;
        if(HarassedBy(from_id))
            return;
        _EventManager.Message(new MessageContents("PRIVATE MESSAGE", username, message, null, null));
    }

    public bool AtPeace(Faction faction){
        return truce_manager.Truced(_SessionManager.OurInstance.FactionData(), faction);
    }

    public bool AreWe(Faction faction){
        return _SessionManager.OurInstance.FactionData() == faction;
    }

    public void MakeWar(Faction target){
        if(!truce_manager.Truced(_SessionManager.OurInstance.FactionData(), target))
            return;

        RPC_MakeWar(_SessionManager.OurInstance.Faction_ID, _FactionLookup.ID(target));
        _PlayerManager.DisableAllTroops();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_MakeWar(int faction_declaring_id, int faction_target_id, RpcInfo info = default){
        
        Faction faction_declaring = _FactionLookup.GetFaction(faction_declaring_id);
        Faction faction_target = _FactionLookup.GetFaction(faction_target_id);

        if(!truce_manager.Truced(faction_declaring, faction_target))
            return;

        truce_manager.BreakTruce(faction_declaring, faction_target);

        TroopsInTerritory(faction_declaring, faction_target);
        
        string[] war_titles = {"WAR DECLARED", "WAR!", "THE GREAT BACKSTAB", "ALLIANCE BREAKS!", "END OF ALL PEACE", "FIRST BLOOD", "CONQUEST BEGINS"};
        string title = war_titles[Random.Range(0, war_titles.Length)];
        _EventManager.Message(new MessageContents("NEWSPAPER", title, "WAR", faction_declaring, faction_target));
        _PlayerManager.LeaderboardWindow.RefreshUI();
        _PlayerManager.FactionInfoWindow.RefreshUI();
    }

    public void TroopsInTerritory(Faction faction_of, Faction faction_in){
        if(AllTroops.Count == 0 || !_SessionManager.Hosting)
            return;

        List<int> locations = new List<int>();
        foreach(Troop troop in AllTroops){
            if(troop.FactionData() == faction_of){
                Tile tile = _MapManager.GetTile(troop.current_tile);
                if(tile.owner == faction_in){
                    _ConnectionManager.Despawn(troop.gameObject.GetComponent<NetworkObject>());
                    locations.Add(tile.ID);
                }
            }
        }

        RPC_SpawnDespawnEffects(locations.ToArray());
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SpawnDespawnEffects(int[] locations, RpcInfo info = default){
        if(locations.Length == 0)
            return;
        foreach(int i in locations){
            _MapManager.SpawnParticleEffect(i);
        }
    }

    public void MakePeace(Faction fac_one, Faction fac_two){
        if(truce_manager.Truced(fac_one, fac_two))
            return;

        Faction our_faction = _SessionManager.OurInstance.FactionData();

        if(our_faction == fac_one)
            harassed_factions[_FactionLookup.ID(fac_two)] = true;
        if(our_faction == fac_two)
            harassed_factions[_FactionLookup.ID(fac_one)] = true;

        RPC_MakePeace(_FactionLookup.ID(fac_one), _FactionLookup.ID(fac_two));
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_MakePeace(int fac_one_id, int fac_two_id){

        Faction faction_one = _FactionLookup.GetFaction(fac_one_id);
        Faction faction_two = _FactionLookup.GetFaction(fac_two_id);

        if(truce_manager.Truced(faction_one, faction_two))
            return;

        truce_manager.NewTruce(faction_one, faction_two);

        string[] peace_titles = {"PEACE AT LAST", "UNEASY TRUCE", "WAR IS OVER", "WORTHY ALLIES"};
        string title = peace_titles[Random.Range(0, peace_titles.Length)];
        _EventManager.Message(new MessageContents("NEWSPAPER", title, "PEACE", faction_one, faction_two));

        _PlayerManager.LeaderboardWindow.RefreshUI();
        _PlayerManager.FactionInfoWindow.RefreshUI();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_OfferTreaty(int offering_faction_id, int target_faction_id, string username){

        Faction fac_offer = _FactionLookup.GetFaction(offering_faction_id);
        Faction fac_targ = _FactionLookup.GetFaction(target_faction_id);

        if(fac_offer == null || fac_targ == null)
            return;        
        if(truce_manager.Truced(fac_offer, fac_targ))
            return;
        if(target_faction_id != _SessionManager.OurInstance.Faction_ID)
            return;
        
        _EventManager.Add(new MessageEvent(new MessageContents("PEACE", username, "", fac_offer, fac_targ)));
        _PlayerManager.LeaderboardWindow.RefreshUI();
        _PlayerManager.FactionInfoWindow.RefreshUI();
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
                if(tiles.Contains(tile) && tile.visible && t.FactionID() != _SessionManager.OurInstance.Faction_ID && !AtPeace(t.FactionData())){
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
    public void RPC_AttackTroop(int attacking_id, int target_id, bool original, int fac_attk, int fac_targ, RpcInfo info = default){
        AttackTroop(attacking_id, target_id, original, fac_attk, fac_targ, GetTroop(target_id).transform.position);
    }

    void AttackTroop(int attacking_id, int target_id, bool original_attack, int fac_attk, int fac_targ, Vector3 old_target_pos){

        Troop attacking_troop = GetTroop(attacking_id);
        Troop target_troop = GetTroop(target_id);

        if(!ValidateTroop(attacking_troop))
            return;
        
        if(truce_manager.Truced(_FactionLookup.GetFaction(fac_attk), _FactionLookup.GetFaction(fac_targ)))
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
                    StartCoroutine(Slapback(attacking_id, target_id, fac_attk, fac_targ));
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

    IEnumerator Slapback(int attacking_id, int target_id, int fac_attk, int fac_targ){
        yield return new WaitForSeconds(0.5f);
        RPC_AttackTroop(target_id, attacking_id, false, fac_attk, fac_targ);
    }

    int CalculateDamage(Troop troop, bool original){
        int damage = troop.Data.Damage();
        if(!original)
            damage = damage / 2;
        if(damage < 1)
            damage = 1;
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
