using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInstance : MonoBehaviour
{
    [SerializeField] int PlayerID;
    [SerializeField] Faction _Faction;
    [SerializeField] FactionLookup _FactionLookup;

    public bool local_player;
    public PlayerManager player_manager;

    public void SnapCameraToPosition(Vector3 position){
        if(local_player && player_manager != null){
            player_manager.SnapCameraToPosition(position);
        }
    }

    public Faction FactionData(){return _Faction;}
}
