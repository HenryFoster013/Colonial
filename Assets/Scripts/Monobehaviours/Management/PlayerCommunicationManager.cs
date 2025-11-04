using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenericUtils;
using EventUtils;
using Fusion;
using TruceUtils;

public class PlayerCommunicationManager : NetworkBehaviour
{
    [Header(" - References - ")]
    [SerializeField] PlayerManager _PlayerManager;
    [SerializeField] SessionManager _SessionManager;
    [SerializeField] GameplayManager _GameplayManager;
    [SerializeField] EventManager _EventManager;
    [SerializeField] TechTreeManager _TechTreeManager;
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] SoundEffectLookup SFX_Lookup;

    [Header(" - Windows - ")]
    [SerializeField] Leaderboard LeaderboardWindow;
    [SerializeField] FactionWindow FactionDisplayWindow;

    public TruceManager truce_manager {get; private set;}

    // COMMON CHECKS //

    public string LocalUsername(){return _SessionManager.OurInstance.GetUsername();}
    public Faction LocalFaction(){return _SessionManager.OurInstance.FactionData();}
    public int LocalFactionID(){return _SessionManager.OurInstance.Faction_ID;}
    public bool AtPeace(Faction faction){return truce_manager.Truced(LocalFaction(), faction);}
    public bool AreWe(Faction faction){return faction == LocalFaction();}
    public bool AreWe(int faction_id){return faction_id == LocalFactionID();}
    
    public bool CanUseFactionUI(Faction target){
        if(!_PlayerManager.OurTurn)
            return false;
        if(Harassed(target))
            return false;
        return true;
    }

    public bool Truced(Faction faction_one, Faction faction_two){return truce_manager.Truced(faction_one, faction_two);}

    // BASE CALLS //

    public void Setup(){
        truce_manager = new TruceManager();
    }

    public void NewTurn(){
        ClearHarassment();
        RefreshAll();
    }

    public void RefreshAll(){
        LeaderboardWindow.RefreshUI();
        FactionDisplayWindow.RefreshUI();
    }

    // SPAM NEGATION //

    // Replace with a hashmap
    bool[] harassed_factions;
    bool[] harassed_by;

    void ClearHarassment(){
        harassed_factions = new bool[_FactionLookup.Length()];
        harassed_by = new bool[_FactionLookup.Length()];
    }

    public bool Harassed(Faction target){return CheckHarassment(_FactionLookup.ID(target), ref harassed_factions);}
    public bool HarassedBy(Faction target){return CheckHarassment(_FactionLookup.ID(target), ref harassed_by);}
    public bool Harassed(int target_id){return CheckHarassment(target_id, ref harassed_factions);}
    public bool HarassedBy(int target_id){return CheckHarassment(target_id, ref harassed_by);}
    bool CheckHarassment(int target_id, ref bool[] reference){
        if(target_id == LocalFactionID())
            return false;
        return reference[target_id];
    }

    public void MarkHarassed(int target){MarkHarrassment(target, ref harassed_factions);}
    public void MarkHarassed(Faction target){MarkHarrassment(_FactionLookup.ID(target), ref harassed_factions);}
    public void MarkHarassedBy(int target){MarkHarrassment(target, ref harassed_by);}
    public void MarkHarassedBy(Faction target){MarkHarrassment(_FactionLookup.ID(target), ref harassed_by);}
    void MarkHarrassment(int target, ref bool[] reference){
        if(target < 0 || target >= reference.Length)
            return;
        reference[target] = true;
    }

    // UI INTERACTIONS //

    public void OpenFactionInformation(Faction faction){
        if(!_PlayerManager.OurTurn || AreWe(faction)){
            PlaySFX("UI_4", SFX_Lookup);
            return;
        }
        FactionDisplayWindow.Load(faction);
    }

    public void LeaderboardButton(){
        LeaderboardWindow.Open();
    }

    public void FlipPeace(Faction target){
        if(!CanUseFactionUI(target))
            return;

        MarkHarassed(target);

        if(!AtPeace(target)){
            if(!_TechTreeManager.Unlocked("OFFER PEACE")){
                PlaySFX("UI_4", SFX_Lookup);
                return;
            }
            OfferPeace(target);
        }
        else
            MakeWar(target);
        
        RefreshAll();
    }

    public void OfferPeace(Faction target){
        if(!truce_manager.Truced(LocalFaction(), target) || !_TechTreeManager.Unlocked("OFFER PEACE"))
            return;
        
        LeaderboardWindow.SilentClose();
        _PlayerManager.DisableAllTroops();
        RPC_OfferTreaty(LocalFactionID(), _FactionLookup.ID(target), LocalUsername());
    }

    public void MakeWar(Faction target){
        if(!truce_manager.Truced(LocalFaction(), target))
            return;
        
        RPC_MakeWar(LocalFactionID(), _FactionLookup.ID(target));
        _PlayerManager.DisableAllTroops();
    }

    public void MakePeace(Faction fac_one, Faction fac_two){
        if(truce_manager.Truced(fac_one, fac_two))
            return;
        
        MarkHarassed(fac_one);
        MarkHarassed(fac_two);         
        RPC_MakePeace(_FactionLookup.ID(fac_one), _FactionLookup.ID(fac_two));
    }

    public void SendMessage(Faction faction, string message){
        int faction_id = _FactionLookup.ID(faction);

        if(Harassed(faction_id) || !ValidateMessage(message, 120) || !_TechTreeManager.Unlocked("TELEGRAPH"))
            return;
        
        MarkHarassed(faction_id);
        RPC_SendMessage(faction_id, LocalFactionID(), message, LocalUsername());
        RefreshAll();
    }

    // RPCS

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendMessage(int faction_id, int from_id, string message, string username){
        if(!AreWe(faction_id))
            return;
        if(!ValidateMessage(message, 120))
            return;
        if(Harassed(from_id))
            return;

        MarkHarassedBy(from_id);
        _EventManager.Message(new MessageContents("PRIVATE MESSAGE", username, message, null, null));
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_MakeWar(int faction_declaring_id, int faction_target_id, RpcInfo info = default){

        Faction faction_declaring = _FactionLookup.GetFaction(faction_declaring_id);
        Faction faction_target = _FactionLookup.GetFaction(faction_target_id);

        if(!truce_manager.Truced(faction_declaring, faction_target))
            return;

        truce_manager.BreakTruce(faction_declaring, faction_target);
        _GameplayManager.DespawnTroopsInTerritory(faction_declaring, faction_target);
        
        string[] war_titles = {"WAR DECLARED", "WAR!", "THE GREAT BACKSTAB", "ALLIANCE BREAKS!", "END OF ALL PEACE", "FIRST BLOOD", "CONQUEST BEGINS"};
        string title = war_titles[Random.Range(0, war_titles.Length)];
        _EventManager.Message(new MessageContents("NEWSPAPER", title, "WAR", faction_declaring, faction_target));
        RefreshAll();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_MakePeace(int fac_one_id, int fac_two_id){

        Faction faction_one = _FactionLookup.GetFaction(fac_one_id);
        Faction faction_two = _FactionLookup.GetFaction(fac_two_id);

        if(faction_one == null || faction_two == null)
            return;   
        if(truce_manager.Truced(faction_one, faction_two))
            return;

        truce_manager.NewTruce(faction_one, faction_two);
        string[] peace_titles = {"PEACE AT LAST", "UNEASY TRUCE", "WAR IS OVER", "WORTHY ALLIES"};
        string title = peace_titles[Random.Range(0, peace_titles.Length)];
        _EventManager.Message(new MessageContents("NEWSPAPER", title, "PEACE", faction_one, faction_two));

        RefreshAll();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_OfferTreaty(int offering_faction_id, int target_faction_id, string username){

        Faction fac_offer = _FactionLookup.GetFaction(offering_faction_id);
        Faction fac_targ = _FactionLookup.GetFaction(target_faction_id);

        if(fac_offer == null || fac_targ == null)
            return;        
        if(truce_manager.Truced(fac_offer, fac_targ))
            return;
        if(!AreWe(target_faction_id))
            return;
        
        _EventManager.Add(new MessageEvent(new MessageContents("PEACE", username, "", fac_offer, fac_targ)));
        RefreshAll();
    }
}
