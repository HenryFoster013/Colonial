using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using Fusion.Sockets;
using Fusion;
using TMPro;

public class SessionManager : MonoBehaviour
{
    [Header(" - MAIN - ")]
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] PreGameManager _PreGameManager;
    [SerializeField] GameplayManager _GameplayManager;
    [SerializeField] PlayerManager _PlayerManager;
    [SerializeField] MapManager _MapManager;
    ConnectionManager _ConnectionManager;
    
    [Header("Player Instances")]
    public List<PlayerInstance> player_instances = new List<PlayerInstance>();
    public PlayerInstance OurInstance { get; private set; }

    public bool Hosting { get; private set; }
    public int game_state { get; private set; } // 0 = Lobby, 1 = Gameplay, 2 = Postmatch
   
    int map_data_recieved;
    bool generated_map, connected;
    
    const float texture_length_pixels = 16;
    MaterialPropertyBlock[] troop_skins;

    // BASE //

    void Start(){
        CheckConnection();
        if(!connected)
            return;
        SetupTroopMaterials();
        InitialSetup();
        AreWeLate();
    }

    void Update(){
        PreGame();
    }

    // SETUP //

    void CheckConnection(){
        GameObject cm_obj = GameObject.FindGameObjectWithTag("Connection Manager");
        connected = (cm_obj != null);
        if(!connected){
            PlayerPrefs.SetString("Error Details", "Bad launch.");
            SceneManager.LoadScene("Network Error");
        }
        else
            _ConnectionManager = cm_obj.GetComponent<ConnectionManager>();
    }

    void InitialSetup(){
        map_data_recieved = 0;
        generated_map = false;
        _ConnectionManager._SessionManager = this;
        Hosting = _ConnectionManager.Hosting();
        _PreGameManager.gameObject.SetActive(true);
        _GameplayManager.gameObject.SetActive(false);
        _PreGameManager.Setup();
    }

    void AreWeLate(){
        if(!_ConnectionManager.Hosting()){
            if(_ConnectionManager.HasGameStarted()){
                PlayerPrefs.SetString("Error Details", "Game already full.");
                _ConnectionManager.DisconnectFromLobby("Network Error");
            }
        }
    }

    // LOBBY GAMEPLAY //

    void PreGame(){
        if(game_state != 0 || !connected)
            return;
        
        GetPlayers();
        _PreGameManager.UpdateUI();
    }

    // GAME START //

    public void HostStartGame(){
        if(!AllPlayersReady())
            return;
        
        game_state = 1;
        _ConnectionManager.CloseOffSession();
        _GameplayManager.DefaultValues();
        GetPlayers();
        ReapplyPlayerIDs();
        SwitchToGameplay();
        HostMakeMap();
    }

    public void ClientStartGame(){
        game_state = 1;
        _MapManager.SetSession(this);
        _GameplayManager.DefaultValues();
        GetPlayers();
        SwitchToGameplay();
        _MapManager.ClientGenerateMap();
    }

    public void SwitchToGameplay(){
        _PreGameManager.CloseUI();
        _PreGameManager.gameObject.SetActive(false);
        _GameplayManager.gameObject.SetActive(true);
        _GameplayManager.SetSession(this);
        _GameplayManager.SetConnection(_ConnectionManager);
        _GameplayManager.SetMap(_MapManager);
        _PlayerManager.Setup(this, _GameplayManager);
    }

    // MAP SETUP //
    
    void HostMakeMap(){
        if(!Hosting)
            return;

        _MapManager.SetSession(this);
        _MapManager.EstablishNoiseMap();
        _MapManager.EstablishOtherRandoms();
        _MapManager.GenerateMap();

        _ConnectionManager.SendRawMapData(0, _MapManager.GrassLimit, _MapManager.GetRawMapData());
        _ConnectionManager.SendLargeIntArray(1, _MapManager.GetTilePieces());
        _ConnectionManager.SendLargeIntArray(2, _MapManager.GetTileOwnership());
    }

    public void GotMapDataRaw(float grass_limit, float[] data){
        if(Hosting)
            return;
        _MapManager.SetGrassLimit(grass_limit);
        _MapManager.SetMapDataRaw(data);
        map_data_recieved++;
        CheckWeHaveAllData();
    }
    
    public void GotTileOwnership(int[] data){
        if(Hosting)
            return;
        _MapManager.SetTileOwnership(data);
        map_data_recieved++;
        CheckWeHaveAllData();
    }

    public void GotTilePieces(int[] data){
        if(Hosting)
            return;
        _MapManager.SetTilePieces(data);
        map_data_recieved++;
        CheckWeHaveAllData();
    }

    void CheckWeHaveAllData(){
        if(Hosting)
            return;
        
        if(map_data_recieved > 2 && !generated_map){
            generated_map = true;
            ClientStartGame();
        }
    }

    // PLAYER SETUP //

    void GetPlayers(){
        GameObject[] player_objects = GameObject.FindGameObjectsWithTag("Player");
        player_instances = new List<PlayerInstance>();
        int initial_faction = -1;
        bool can_war = false;
        bool all_ready = true;

        for(int i = 0; i < player_objects.Length; i++){
            PlayerInstance player_inst_temp = player_objects[i].GetComponent<PlayerInstance>();

            player_instances.Add(player_inst_temp);
            NetworkObject NO = player_instances[i].GetComponent<NetworkObject>();

            if(!player_instances[i].Ready)
                all_ready = false;

            if(initial_faction == -1)
                initial_faction = player_instances[i].Faction_ID;

            if(initial_faction != player_instances[i].Faction_ID)
                can_war = true;

            if(NO.HasInputAuthority){
                OurInstance = player_instances[i];
                player_instances[i].SetManager(_PlayerManager);
            }
        }

        player_instances = player_instances.OrderBy(p => p.ID).ToList();

        _PreGameManager.SetStartButtons(can_war, all_ready);
    }

    void ReapplyPlayerIDs(){
        for(int i = 0; i < player_instances.Count; i++){
            player_instances[i].SetID(i);
        }
    }

    public Faction PlayerFaction(int player_id){
        return player_instances[player_id].FactionData();
    }

    public int PlayerFactionID(int player_id){
        return player_instances[player_id].Faction_ID;
    }

    public bool AllPlayersReady(){
        bool ready = true;
        foreach(PlayerInstance pi in player_instances){
            if(!pi.Ready)
                ready = false;
        }
        return ready;
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

    // GETTERS //

    public MaterialPropertyBlock[] GetTroopMaterials(int faction_id){
        MaterialPropertyBlock[] return_mats = new MaterialPropertyBlock[2];
        return_mats[0] = troop_skins[faction_id * 2];
        return_mats[1] = troop_skins[(faction_id * 2) + 1];
        return return_mats;
    }

    public PlayerInstance GetPlayer(int i){return player_instances[i];}
    public int LocalFactionID(){return _FactionLookup.ID(OurInstance.FactionData());}
    public Faction LocalFactionData(){return OurInstance.FactionData();}
    public int GetPlayerCount(){return player_instances.Count;}
    public List<PlayerInstance> GetAllPlayerInstances(){return player_instances;}
}
