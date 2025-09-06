using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Troop : NetworkBehaviour{
    
    [Header(" - MAIN - ")]
    public TroopData Data;
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] Collider Col;

    [Header(" - DISPLAY - ")]
    [SerializeField] GameObject ModelHolder;
    [SerializeField] Animator Anim;
    public Material BaseMaterial;
    [SerializeField] MeshRenderer[] Meshes;
    
    [Networked] public int Owner {get; set;}
    [Networked] public int Faction_ID {get; set;}
    [Networked] public bool Ready_Marker {get; set;}
    [Networked] public int current_tile {get; set;}
    
    MapManager _MapManager;
    SessionManager _SessionManager;
    Faction faction;
    
    MaterialPropertyBlock[] skins;
    const int mesh_default_layer = 0;
    const int mesh_highlight_layer = 9;
    int tile_buffer;
    bool used_move, used_special, setup, spawned;

    // SETUP //

    void Start(){Setup();}
    public override void Spawned(){spawned = true;}

    void Update(){
        CheckForDataSetup();
        CheckForMovement();
    }

    void Setup(){
        transform.eulerAngles = new Vector3(0f, 90f, 0f);
        _SessionManager = GameObject.FindGameObjectWithTag("Session Manager").GetComponent<SessionManager>();
        _MapManager = GameObject.FindGameObjectWithTag("Map Manager").GetComponent<MapManager>();
        used_move = false;
        used_special = true;
        ModelHolder.SetActive(false);
    }

    void CheckForDataSetup(){
        if(!setup && spawned){
            if(Ready_Marker)
                GotDataSetup();
        }
    }

    void CheckForMovement(){
        if(!Ready_Marker)
            return;
        
        if(current_tile != tile_buffer)
            SetPosition();
    }
    
    void GotDataSetup(){
        setup = true;
        faction = _FactionLookup.GetFaction(Faction_ID);
        SetupMaterial();
        ModelHolder.SetActive(true);
        SetPosition();
    }

    void SetupMaterial(){
        skins = _SessionManager.GetTroopMaterials(Faction_ID);
        UpdateModel();
    }

    // MOVEMENT //

    void SetPosition(){
        //if(used_move)
        //    return;

        tile_buffer = current_tile;
        Anim.Play("Hop", -1, 0);
        Vector3 new_pos = _MapManager.GetTroopPosition(current_tile);
        transform.LookAt(new Vector3(new_pos.x,transform.position.y,new_pos.z));
        transform.position = new_pos;

        if(Owner == _SessionManager.OurInstance.ID){
            _MapManager.MarkRadiusAsVisible(current_tile, Data.Vision());
            _MapManager.CheckForMapRegen();
        }
        
        //UseMove();  
    }

    // GRAPHICS //

    void UpdateModel(){
        if(Owner != _SessionManager.OurInstance.ID)
            SetSkin(mesh_default_layer, 0);
        else if(TurnOver())
            SetSkin(mesh_default_layer, 1);
        else
            SetSkin(mesh_highlight_layer, 0);
    }

    void SetSkin(int layer, int state){
        foreach(MeshRenderer renderer in Meshes){
            renderer.SetPropertyBlock(skins[state]);
            renderer.gameObject.layer = layer;
        }
    }

    // TURN LOGIC //

    public void NewTurn(){
        used_move = false;
        used_special = true;
        UpdateModel();
    }

    public void UseMove(){
        used_move = true;
        EndTurn();
    }

    public void UseSpecial(){
        used_special = true;
        EndTurn();
    }

    void EndTurn(){
        if(!TurnOver())
            return;
        
        // Run out of things to do logic
        UpdateModel();
    }

    // GETTERS AND SETTERS //

    public bool UsedMove(){return used_move;}
    public bool UsedSpecial(){return used_special;}
    public bool TurnOver(){return (used_move && used_special);}
    public int GetTile(){return current_tile;}
    public Faction FactionData(){return faction;}
    public int FactionID(){return Faction_ID;}
}
