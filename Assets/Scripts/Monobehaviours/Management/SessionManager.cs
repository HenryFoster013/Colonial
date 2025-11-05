using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using Fusion.Sockets;
using Fusion;
using TMPro;
using MapUtils;

public class SessionManager : NetworkBehaviour
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

    public bool Hosting { get; private set;}
    public int game_state { get; private set;} // 0 = Lobby, 1 = Gameplay, 2 = Postmatch

    [Header("Backgrounds")]
    [SerializeField] BackgroundColouring PregameBackground;
   
    int map_data_recieved;
    bool connected;
    
    const float texture_width_pixels = 16;
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
        MainGame();
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
        
        _ConnectionManager.CloseOffSession();
        _MapManager.seed = new Seed();
        RPC_StartGame(Random.Range(0,999999999));
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_StartGame(int seed_value, RpcInfo info = default){
        _MapManager.seed = new Seed(seed_value);
        ReapplyPlayerIDs();
        StartGame();
    }

    void StartGame(){
        game_state = 1;
        _GameplayManager.Setup();
        GetPlayers();
        SwitchToGameplay();
        _MapManager.EstablishNoiseMap();
        _MapManager.EstablishOtherRandoms();
        _MapManager.GenerateMap();
    }

    public void SwitchToGameplay(){
        _PreGameManager.CloseUI();
        _PreGameManager.gameObject.SetActive(false);
        _GameplayManager.gameObject.SetActive(true);
        _GameplayManager.SetConnection(_ConnectionManager);
        _PlayerManager.Setup();
        PregameBackground.Save();
    }

    // PLAYER SETUP //

    void GetPlayers(){
        GameObject[] player_objects = GameObject.FindGameObjectsWithTag("Player");
        player_instances = new List<PlayerInstance>();
        int initial_faction = -1;
        bool can_war = true;
        bool all_ready = true;
        List<int> faction_ids = new List<int>();

        for(int i = 0; i < player_objects.Length; i++){
            PlayerInstance player_inst_temp = player_objects[i].GetComponent<PlayerInstance>();

            player_instances.Add(player_inst_temp);
            NetworkObject NO = player_instances[i].GetComponent<NetworkObject>();

            if(!player_instances[i].Ready)
                all_ready = false;

            if(initial_faction == -1)
                initial_faction = player_instances[i].Faction_ID;

            if(faction_ids.Contains(initial_faction))
                can_war = false;
            else
                faction_ids.Add(initial_faction);

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
        float base_offset = -2f / texture_width_pixels; // Size of one colouring (2px) divided by the size of the image,  * -1 as goes down
        troop_skins = new MaterialPropertyBlock[_FactionLookup.Length() * 2];
        for(int i = 0; i < _FactionLookup.Length(); i++){

            offy = 0.5f + _FactionLookup.GetFaction(i).TextureOffset() * base_offset;
            MaterialPropertyBlock default_skin = new MaterialPropertyBlock();
            default_skin.SetVector("offy", new Vector2(0, offy));
            default_skin.SetVector("tiling", new Vector2(1, 0.5f));
            MaterialPropertyBlock disabled_skin = new MaterialPropertyBlock();
            disabled_skin.SetVector("offy", new Vector2(0, offy + (base_offset / 2)));
            disabled_skin.SetVector("tiling", new Vector2(1, 0.5f));

            troop_skins[i * 2] = default_skin;
            troop_skins[(i * 2) + 1] = disabled_skin;
        }
    }

    // MAIN GAME //

    void MainGame(){
        if(game_state != 1)
            return;
        
        CheckPlayers();
    }

    void CheckPlayers(){
        bool player_gone = false;
        foreach(PlayerInstance pi in player_instances){
            if(pi == null)
                player_gone = true;
        }

        if(player_gone){
            PlayerPrefs.SetString("Error Details", "Opponent suddenly disconnected.");
            _ConnectionManager.DisconnectFromLobby("Network Error");
        }
    }

    // MONEY //

    public int Money(){return OurInstance.Money();}
    public void SetMoney(int money){OurInstance.SetMoney(money);}
    public void EarnMoney(int money){OurInstance.EarnMoney(money);}
    public void SpendMoney(int money){OurInstance.SpendMoney(money);}

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
