using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion.Sockets;
using Fusion;

public class SessionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] MapManager _MapManager;
    [SerializeField] PlayerManager _PlayerManager;
    [Header("Player Instances")]
    [SerializeField] PlayerInstance OurInstance;
    public PlayerInstance[] player_instances;
    [Header("Lobby")]
    [SerializeField] GameObject LobbyMaster;
    [SerializeField] GameObject L_HostOnly;
    
    // 0 = Lobby
    // 1 = Gameplay
    // 2 = Postmatch
    int game_state = 0;

    bool game_host = false;
    ConnectionManager _ConnectionManager;
    int map_data_recieved;
    bool generated_map = false;

    int current_turn = 0;
    int current_stars = 3;
    int stars_per_turn = 3;
    
    const float texture_length_pixels = 16;
    MaterialPropertyBlock[] troop_skins;

    // BASE //

    void Start(){
        SetupTroopMaterials();
        LobbySetup();
        AreWeLate();
    }

    void Update(){
        LobbyLogic();
        CheckConnection();
    }

    void CheckConnection(){
        if(_ConnectionManager == null)
            SceneManager.LoadScene("Network Error");
    }

    void AreWeLate(){
        if(!_ConnectionManager.AreWeHost()){
            if(_ConnectionManager.HasGameStarted()){
                PlayerPrefs.SetString("Error Details", "Game already full.");
                _ConnectionManager.DisconnectFromLobby("Network Error");
            }
        }
    }

    // LOBBY GAMEPLAY //

    void LobbyLogic(){
        if(game_state != 0)
            return;
        
        GetPlayers();
    }

    void LobbySetup(){
        map_data_recieved = 0;
        generated_map = false;
        _ConnectionManager = GameObject.FindGameObjectWithTag("Connection Manager").GetComponent<ConnectionManager>();
        _ConnectionManager._SessionManager = this;
        _PlayerManager.transform.parent.gameObject.SetActive(false);
        game_host = _ConnectionManager.AreWeHost();
        LobbyMaster.SetActive(true);
        L_HostOnly.SetActive(game_host);
    }

    void CloseLobbyUI(){
        LobbyMaster.SetActive(false);
    }

    // MAIN GAMEPLAY //

    public void HostStartGame(){
        _ConnectionManager.CloseOffSession();
        CloseLobbyUI();
        DefaultValues();
        GetPlayers();
        InitialisePlayerManager();
        MakeMap();
    }

    public void ClientStartGame(){
        _MapManager.SetSession(this);
        GetPlayers();
        CloseLobbyUI();
        DefaultValues();
        InitialisePlayerManager();
        _MapManager.ClientGenerateMap();
    }

    void DefaultValues(){
        current_stars = 3;
        current_turn = 1;
        game_state = 1;
    }

    void InitialisePlayerManager(){
        _PlayerManager.transform.parent.gameObject.SetActive(true);
        _PlayerManager.Setup();
    }

    // Map Setup //
    
    void MakeMap(){
        _MapManager.SetSession(this);

        if(game_host){ // Otherwise, will have to download the perline noise and piece data
            _MapManager.EstablishNoiseMap();
            _MapManager.EstablishOtherRandoms();
        }

        _MapManager.GenerateMap();

        _ConnectionManager.SendLargeFloatArray(0, _MapManager.GetRawMapData());
        _ConnectionManager.SendLargeIntArray(1, _MapManager.GetTilePieces());
        _ConnectionManager.SendLargeIntArray(2, _MapManager.GetTileOwnership());
    }

    public void GotMapDataRaw(float[] data){
        if(game_host)
            return;
        _MapManager.SetMapDataRaw(data);
        map_data_recieved++;
        CheckWeHaveAllData();
    }

    public void GotTileOwnership(int[] data){
        if(game_host)
            return;
        _MapManager.SetTileOwnership(data);
        map_data_recieved++;
        CheckWeHaveAllData();
    }

    public void GotTilePieces(int[] data){
        if(game_host)
            return;
        _MapManager.SetTilePieces(data);
        map_data_recieved++;
        CheckWeHaveAllData();
    }

    void CheckWeHaveAllData(){
        if(game_host)
            return;
        
        if(map_data_recieved > 2 && !generated_map){
            print("Data Constructed");
            generated_map = true;
            ClientStartGame();
        }
    }

    // Player Setup //

    void GetPlayers(){
        GameObject[] player_objects = GameObject.FindGameObjectsWithTag("Player");
        player_instances = new PlayerInstance[player_objects.Length];

        for(int i = 0; i < player_objects.Length; i++){

            player_instances[i] = player_objects[i].GetComponent<PlayerInstance>();
            NetworkObject NO = player_instances[i].GetComponent<NetworkObject>();

            player_instances[i].SetOwner(NO.InputAuthority);
            player_instances[i].SetLocal(NO.HasInputAuthority);

            if(NO.HasInputAuthority){
                OurInstance = player_instances[i];
                player_instances[i].SetManager(_PlayerManager);
            }
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

    public PlayerInstance[] GetAllPlayerInstances(){
        return player_instances;
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
