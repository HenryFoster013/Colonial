using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerInstance : NetworkBehaviour{

    [Networked] public int Faction_ID { get; set; }
    [Networked, Capacity(32)] public string Username { get; set; }
    [Networked] public bool Host { get; set; }
    [Networked] public bool Ready { get; set; }
    

    [SerializeField] FactionLookup _FactionLookup;
    PlayerManager player_manager;
    private ChangeDetector _changeDetector;

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

    // Setters //
    public void SetManager(PlayerManager manager){player_manager = manager;}
    public void WeAreHost(){Host = true;}

    // Getters //
    public Faction FactionData(){return _FactionLookup.GetFaction(Faction_ID);}
}
