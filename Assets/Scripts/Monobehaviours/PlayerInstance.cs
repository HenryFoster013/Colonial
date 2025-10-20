using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerInstance : NetworkBehaviour{

    [Networked] public int Faction_ID {get; set;}
    [Networked, Capacity(32)] public string Username {get; set;}
    [Networked] public bool Host {get; set;}
    [Networked] public bool Ready {get; set;}
    [Networked] public int ID {get; set;}

    [SerializeField] FactionLookup _FactionLookup;

    PlayerManager player_manager;
    private ChangeDetector _changeDetector;
    int current_currency;

    public override void Spawned(){
        if(Object.HasInputAuthority){
            Username = PlayerPrefs.GetString("USERNAME");
            Faction_ID = PlayerPrefs.GetInt("FACTION");
            RPC_SetCoreData(PlayerPrefs.GetString("USERNAME"), PlayerPrefs.GetInt("FACTION"));
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetCoreData(string username, int faction_id, RpcInfo info = default){
        Username = username;
        Faction_ID = faction_id;
        Ready = true;
    }

    public void UpdateFaction(){
        if(Object.HasInputAuthority){
            RPC_SetCoreData(PlayerPrefs.GetString("USERNAME"), PlayerPrefs.GetInt("FACTION"));
        }
    }

    public void SnapCameraToPosition(Vector3 position){
        if(Object.HasInputAuthority && player_manager != null){
            player_manager.SnapCameraToPosition(position);
        }
    }

    public string FilterText(string text, int length){

        // Apply censors here
        
        if(text.Length > length){
            text = text.Substring(0, length);
        }

        return text;
    }

    // Setters //
    public void SetManager(PlayerManager manager){player_manager = manager;}
    public void WeAreHost(){Host = true;}
    public void SetID(int id){ID = id;}
    public void SetMoney(int money){current_currency = money;}
    public void SpendMoney(int money){current_currency -= money;}
    public void EarnMoney(int money){current_currency += money;}

    // Getters //
    public Faction FactionData(){return _FactionLookup.GetFaction(Faction_ID);}
    public string GetUsername(){return FilterText(Username, 24);}
    public int Money(){return current_currency;}
}
