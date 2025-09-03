using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] MapManager _MapManager;
    [SerializeField] PlayerManager _PlayerManager;
    [Header("Player Instances")]
    [SerializeField] PlayerInstance OurInstance;
    
    int current_turn = 0;
    int current_stars = 3;
    int stars_per_turn = 3;
    bool local = true;
    
    PlayerInstance[] player_instances;
    const float texture_length_pixels = 16;
    MaterialPropertyBlock[] troop_skins;

    // Lobby Setup //

    void Start(){
        SetupTroopMaterials();
        LobbySetup();
    }

    void LobbySetup(){
        _PlayerManager.transform.parent.gameObject.SetActive(false);
    }

    // Game Setup //

    public void StartGame(){
        DefaultValues();
        GetPlayers();
        MakeMap();
        InitialisePlayer();
    }

    void DefaultValues(){
        current_stars = 3;
        current_turn = 1;
    }

    void InitialisePlayer(){
        _PlayerManager.transform.parent.gameObject.SetActive(true);
        _PlayerManager.Setup();
    }

    // Map Setup //
    
    void MakeMap(){
        _MapManager.SetSession(this);

        if(local){ // Otherwise, will have to download the perline noise and piece data
            _MapManager.EstablishNoiseMap();
            _MapManager.EstablishOtherRandoms();
        }

        _MapManager.GenerateMap();
    }

    // Player Setup //

    void GetPlayers(){
        GameObject[] player_objects = GameObject.FindGameObjectsWithTag("Player");
        player_instances = new PlayerInstance[player_objects.Length];
        for(int i = 0; i < player_objects.Length; i++){
            player_instances[i] = player_objects[i].GetComponent<PlayerInstance>();
        }
    }

    void SetupTroopMaterials(){
        float offy = 0;
        float base_offset = -2f / texture_length_pixels; // Size of one colouring (2px) divided by the size of the image,  * -1 as goes down
        troop_skins = new MaterialPropertyBlock[_FactionLookup.Length() * 2];
        for(int i = 0; i < _FactionLookup.Length(); i++){

            offy = _FactionLookup.GetFaction(i).TextureOffset() * base_offset * 2f;
            MaterialPropertyBlock default_skin = new MaterialPropertyBlock();
            default_skin.SetVector("offy", new Vector2(0, offy));
            MaterialPropertyBlock disabled_skin = new MaterialPropertyBlock();
            disabled_skin.SetVector("offy", new Vector2(0, offy + base_offset));

            troop_skins[i * 2] = default_skin;
            troop_skins[(i * 2) + 1] = disabled_skin;
        }
    }

    // Local Communication //

    public Troop SpawnLocalTroop(TroopData troop_data, int tile){
        print(tile);

        if(!_MapManager.CheckTileOwnership(tile, LocalPlayerFaction()))
            return null;

        GameObject g = GameObject.Instantiate(troop_data.Prefab(), Vector3.zero, Quaternion.identity);
        Troop troop = g.GetComponent<Troop>();
        troop.InitialSetup(this, _MapManager, _FactionLookup, OurInstance.FactionData());
        troop.SetPosition(tile);

        return troop;
    }

    public void MoveLocalTroop(Troop troop, int id){
        troop.SetPosition(id);
    }

    // Turns //

    public void UpTurn(){
        current_turn++;
    }

    public void UpStars(){
        current_stars += stars_per_turn;
    }

    public void SpendStars(int cost){
        current_stars -= cost;
    }

    // GETTERS //

    public PlayerInstance GetPlayer(int i){
        return player_instances[i];
    }

    public int LocalPlayerFaction(){
        return _FactionLookup.ID(OurInstance.FactionData());
    }

    public int GetPlayerCount(){
        return player_instances.Length;
    }

    public PlayerInstance LocalInstance(){
        return OurInstance;
    }

    public MaterialPropertyBlock[] GetTroopMaterials(int faction_id){
        MaterialPropertyBlock[] return_mats = new MaterialPropertyBlock[2];
        return_mats[0] = troop_skins[faction_id * 2];
        return_mats[1] = troop_skins[(faction_id * 2) + 1];
        return return_mats;
    }

    public int CurrentTurn(){
        return current_turn;
    }

    public int CurrentStars(){
        return current_stars;
    }
}
