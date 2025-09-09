using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static HenrysUtils;

public class Troop : NetworkBehaviour{
    
    [Header(" - MAIN - ")]
    public TroopData Data;
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] SoundEffectLookup SFX_Lookup;
    [SerializeField] Collider Col;

    [Header(" - DISPLAY - ")]
    [SerializeField] GameObject[] ModelHolders;
    [SerializeField] Animator Anim;
    public Material BaseMaterial;
    [SerializeField] MeshRenderer[] Meshes;
    
    [Networked] public int Owner {get; set;}
    [Networked] public int Faction_ID {get; set;}
    [Networked] public int UniqueID {get; set;}
    [Networked] public int current_tile {get; set;}
    
    MapManager _MapManager;
    SessionManager _SessionManager;
    PlayerManager _PlayerManager;
    GameplayManager _GameplayManager;
    Faction faction;
    
    MaterialPropertyBlock[] skins;
    const int mesh_default_layer = 0;
    const int mesh_highlight_layer = 9;
    public int tile_buffer = -1;
    bool used_move, used_special, setup, spawned, first_move_completed, selected;

    // SETUP //

    void Start(){Setup();}
    public override void Spawned(){spawned = true;}

    void Update(){
        CheckForDataSetup();
        CheckForMovement();
        CheckVisibility();
    }

    // SETUP //

    void Setup(){
        foreach(GameObject model_holder in ModelHolders)
            model_holder.SetActive(false);
        transform.eulerAngles = new Vector3(0f, 90f, 0f);
        _SessionManager = GameObject.FindGameObjectWithTag("Session Manager").GetComponent<SessionManager>();
        _PlayerManager = GameObject.FindGameObjectWithTag("Player Manager").GetComponent<PlayerManager>();
        _MapManager = GameObject.FindGameObjectWithTag("Map Manager").GetComponent<MapManager>();
        _GameplayManager = GameObject.FindGameObjectWithTag("Gameplay Manager").GetComponent<GameplayManager>();
        _GameplayManager.AddTroop(this);
        used_move = false;
        used_special = true;
        first_move_completed = false;
        selected = false;
        Anim.SetBool("selected", selected);
    }

    void CheckForDataSetup(){
        if(!setup && spawned){
            if(UniqueID != 0)
                GotDataSetup();
        }
    }
    
    void GotDataSetup(){
        _PlayerManager.AddTroop(this);
        setup = true;
        faction = _FactionLookup.GetFaction(Faction_ID);
        SetupMaterial();
    }

    void SetupMaterial(){
        skins = _SessionManager.GetTroopMaterials(Faction_ID);
        UpdateModel();
    }

    // MOVEMENT //

    public void SetSelected(bool select){
        if(UniqueID == 0)
            return;   

        selected = select;
        Anim.SetBool("selected", selected);
    }

    void CheckForMovement(){
        if(UniqueID == 0)
            return;    
        if(current_tile != tile_buffer)
            SetPosition();
    }

    void SetPosition(){
        //if(used_move)
        //    return;

        first_move_completed = true;
        tile_buffer = current_tile;
        Anim.Play("Hop", -1, 0);
        Vector3 new_pos = _MapManager.GetTroopPosition(current_tile);
        transform.LookAt(new Vector3(new_pos.x,transform.position.y,new_pos.z));
        transform.position = new_pos;

        if(_SessionManager.OurInstance.ID == Owner){
            _MapManager.MarkRadiusAsVisible(current_tile, Data.Vision());
            _MapManager.CheckForMapRegen();
        }

        if(_MapManager.CheckVisibility(current_tile))
            PlaySFX("Placement", SFX_Lookup);

        //UseMove();  
    }

    // GRAPHICS //

    public void CheckVisibility(){
        if(UniqueID == 0)
            return;   

        bool visible = setup && first_move_completed && _MapManager.CheckVisibility(current_tile);
        foreach(GameObject model_holder in ModelHolders)
            model_holder.SetActive(visible);
        Col.enabled = visible;
    }

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
    public Faction FactionData(){return faction;}
    public int FactionID(){return Faction_ID;}
}
