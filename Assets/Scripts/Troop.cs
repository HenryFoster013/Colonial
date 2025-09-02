using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Troop : MonoBehaviour{

    [Header(" - MAIN - ")]
    public TroopData Data;
    [SerializeField] Collider Col;

    [Header(" - DISPLAY - ")]
    [SerializeField] Animator Anim;
    public Material BaseMaterial;
    [SerializeField] MeshRenderer[] Meshes;
    
    MapManager map_manager;
    SessionManager session_manager;
    FactionLookup faction_lookup;

    Faction faction;

    MaterialPropertyBlock[] skins;
    const int mesh_default_layer = 0;
    const int mesh_highlight_layer = 9;

    int current_tile;
    bool used_move = false;
    bool used_special = false;
    

    // MAIN LOGIC //

    void Start(){
        transform.eulerAngles = new Vector3(0f, 90f, 0f);
    }

    public void InitialSetup(SessionManager sm, MapManager map, FactionLookup fm, Faction fac){
        session_manager = sm;
        map_manager = map;
        faction_lookup = fm;
        faction = fac;
        
        transform.eulerAngles = new Vector3(0f, 90f, 0f);
        used_move = true;
        used_special = true;

        SetupMaterial();
    }

    void SetupMaterial(){
        skins = session_manager.GetTroopMaterials(faction_lookup.ID(faction));
        UpdateModel();
    }

    public void DisplayInitialSetup(SessionManager sm, int faction_id, int hidden_layer){
        skins = sm.GetTroopMaterials(faction_id);
        SetSkin(hidden_layer, 0);
    }

    public void SetPosition(int i){
        if(used_move)
            return;

        current_tile = i;
        Anim.Play("Hop", -1, 0);
        map_manager.MarkRadiusAsVisible(i, Data.Vision());
        map_manager.CheckForMapRegen();

        Vector3 new_pos = map_manager.GetTroopPosition(current_tile);
        transform.LookAt(new Vector3(new_pos.x,transform.position.y,new_pos.z));
        transform.position = new_pos;
        
        UseMove();  
    }

    // GRAPHICS //

    void UpdateModel(){

        int layer = mesh_highlight_layer;
        int state = 0; // default

        if(TurnOver()){
            layer = mesh_default_layer;
            state = 1; // desaturated
        }

        SetSkin(layer, state);
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
    public int FactionID(){return faction_lookup.ID(faction);}
}
