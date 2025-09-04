using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerInstance : MonoBehaviour{
    
    [SerializeField] Faction _Faction;
    [SerializeField] FactionLookup _FactionLookup;

    bool local_player;
    PlayerManager player_manager;
    PlayerRef Owner;

    public void SnapCameraToPosition(Vector3 position){
        if(local_player && player_manager != null){
            player_manager.SnapCameraToPosition(position);
        }
    }

    // Setters //
    public void SetOwner(PlayerRef owner){Owner = owner;}
    public void SetManager(PlayerManager manager){player_manager = manager;}
    public void SetLocal(bool local){local_player = local;}

    // Getters //
    public Faction FactionData(){return _Faction;}
    public PlayerRef GetOwner(){return Owner;}
}
