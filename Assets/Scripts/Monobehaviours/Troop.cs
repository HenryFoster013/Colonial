using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;
using static HenrysUtils;

public class Troop : NetworkBehaviour{
    
    [Header(" - MAIN - ")]
    public TroopData Data;
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] SoundEffectLookup SFX_Lookup;
    [SerializeField] Collider Col;

    [Header(" - UI - ")]
    [SerializeField] GameObject UI_Holder;
    [SerializeField] Image Health_BG;
    [SerializeField] TMP_Text HealthText;

    [Header(" - MODEL - ")]
    [SerializeField] Animator Anim;
    public Material BaseMaterial;
    [SerializeField] MeshRenderer[] Meshes;
    [SerializeField] GameObject Shadow;
    
    [Networked] public int Owner {get; set;}
    [Networked] public int Faction_ID {get; set;}
    [Networked] public int UniqueID {get; set;}
    [Networked] public int current_tile {get; set;}
    [Networked] public int health {get; set;}
    
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
        HealthText.text = health.ToString();
    }

    // SETUP //

    void Setup(){

        _SessionManager = GameObject.FindGameObjectWithTag("Session Manager").GetComponent<SessionManager>();
        _PlayerManager = GameObject.FindGameObjectWithTag("Player Manager").GetComponent<PlayerManager>();
        _MapManager = GameObject.FindGameObjectWithTag("Map Manager").GetComponent<MapManager>();
        _GameplayManager = GameObject.FindGameObjectWithTag("Gameplay Manager").GetComponent<GameplayManager>();

        _GameplayManager.AddTroop(this);
        health = Data.Health();
        
        transform.eulerAngles = new Vector3(0f, 90f, 0f);
        DisplayModel(false);
        Anim.SetBool("selected", selected);

        used_move = true;
        used_special = true;
        first_move_completed = false;
        selected = false;
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
        Health_BG.color = FactionData().Colour();
        SetupMaterial();
        UpdateModel();
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
        first_move_completed = true;
        tile_buffer = current_tile;
        Anim.Play("Hop", -1, 0);
        RotateAt(_MapManager.GetTroopPosition(current_tile));
        transform.position = _MapManager.GetTroopPosition(current_tile);

        if(_SessionManager.OurInstance.ID == Owner){
            _MapManager.MarkRadiusAsVisible(current_tile, Data.Vision());
            _MapManager.CheckForMapRegen();
        }

        if(_MapManager.CheckVisibility(current_tile))
            PlaySFX("Placement", SFX_Lookup);
        
        if(_PlayerManager.CheckNoSpecials(this)){
            UseSpecial();
        }
    }

    // GRAPHICS //

    public void CheckVisibility(){
        if(UniqueID == 0)
            return;   

        bool visible = setup && first_move_completed && _MapManager.CheckVisibility(current_tile);
        
        Col.enabled = visible;
        DisplayModel(visible);
    }

    void DisplayModel(bool visible){
        Shadow.SetActive(visible);
        UI_Holder.SetActive(visible);
        foreach(MeshRenderer mr in Meshes)
            mr.gameObject.SetActive(visible);
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

    // COMBAT //

    public void AttackAnim(){
        Anim.Play(Data.AttackAnim());
        PlaySFX("Drums_5", SFX_Lookup);
    }

    public void DamageAnim(){
        Anim.Play("TroopDamaged");
    }

    public void RotateAt(Vector3 pos){
        Vector3 new_look = new Vector3(pos.x, transform.position.y, pos.z);
        transform.LookAt(new_look);
    }

    // TURN LOGIC //

    public void NewTurn(){
        used_move = false;
        used_special = false;
        UpdateModel();
    }

    public void UseMove(){
        used_move = true;
        UpdateModel();
    }

    public void UseSpecial(){
        used_special = true;
        UpdateModel();
    }

    public void EndTurn(){
        UseMove();
        UseSpecial();
    }

    // GETTERS AND SETTERS //

    public bool UsedMove(){return used_move;}
    public bool UsedSpecial(){return used_special;}
    public bool TurnOver(){return (used_move && used_special);}
    public Faction FactionData(){return faction;}
    public int FactionID(){return Faction_ID;}
}
