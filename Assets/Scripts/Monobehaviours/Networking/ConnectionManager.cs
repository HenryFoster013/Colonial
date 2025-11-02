using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random=System.Random;

public class ConnectionManager : MonoBehaviour, INetworkRunnerCallbacks{
    
    [Header("Main")]
    public NetworkPrefabRef PlayerInstancePrefab;

    const int GameplaySceneIndex = 1;
    [HideInInspector] public LobbyUI _LobbyUI;
    [HideInInspector] public AutoMatchmake _AutoMatchmake;
    [HideInInspector] public SessionManager _SessionManager;
    [SerializeField] BackgroundColouring BackgroundColours;
    
    private NetworkRunner _runner;
    private static Random random = new Random();
    public bool connected_to_lobby {get; private set;}
    int key_counter, player_counter;
    string password_buffer;

    // Session //

    public bool HasGameStarted(){
        return _runner.SessionInfo.Properties["Game_Started"];
    }

    public void CloseOffSession(){
        if(_runner.IsServer){
            var updatedProps = new Dictionary<string, SessionProperty>()
            {
                {"Owner", PlayerPrefs.GetString("USERNAME")},
                {"Password", password_buffer},
                {"Game_Started", true}
            };
            _runner.SessionInfo.UpdateCustomProperties(updatedProps);
        }
    }

    public void Despawn(NetworkObject NO){
        _runner.Despawn(NO);
    }

    public void SendLargeIntArray(int header, int[] array){

        byte[] intBytes = new byte[array.Length * sizeof(int)];
        Buffer.BlockCopy(array, 0, intBytes, 0, intBytes.Length);

        byte[] data = new byte[sizeof(int) + intBytes.Length];
        Buffer.BlockCopy(System.BitConverter.GetBytes(header), 0, data, 0, sizeof(int));
        Buffer.BlockCopy(intBytes, 0, data, sizeof(int), intBytes.Length);

        foreach(PlayerRef player in _runner.ActivePlayers){
            if(player != _runner.LocalPlayer){
                _runner.SendReliableDataToPlayer(player, ReliableKey.FromInts(key_counter, 0, 0, 0), data);
                key_counter++;
            }
        }
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){
        int offset = data.Offset;

        // Read Type
        int type = System.BitConverter.ToInt32(data.Array, offset);
        offset += sizeof(int);
        
        if(type == 0){ // floats

            int data_count = (data.Count - sizeof(int) - sizeof(int) - sizeof(float)) / sizeof(float);
            float[] decompressed_data = new float[data_count];
            Buffer.BlockCopy(data.Array, offset, decompressed_data, 0, data.Count - sizeof(float) - sizeof(int) - sizeof(int));
        }

        if(type == 1){ // Integers
            int data_count = (data.Count - sizeof(int)) / sizeof(int);
            int[] decompressed_data = new int[data_count];
            Buffer.BlockCopy(data.Array, offset, decompressed_data, 0, data.Count - sizeof(int));
        }
    }

    // Lobbies //

    public async void ConnectToLobby(){
        connected_to_lobby = false;
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        RefreshLobbyConnection();
    }

    public async void DisconnectFromLobby(string next_scene){
        await _runner.Shutdown();
        SceneManager.LoadScene(next_scene);
    }

    public async void RefreshLobbyConnection(){
        connected_to_lobby = false;
        UpdateLobbyUI();
        await System.Threading.Tasks.Task.Delay(100);
        await _runner.JoinSessionLobby(SessionLobby.Shared);
        connected_to_lobby = true;
        UpdateLobbyUI();
    }

    // Lobby UI Interactions (Server Browser) //

    void UpdateLobbyContentSize(){
        if(_LobbyUI != null)
            _LobbyUI.UpdateContentSize();
    }

    void UpdateLobbyUI(){
        if(_LobbyUI != null)
            _LobbyUI.UpdateLobbyUI();
    }

    // Session Hosting and Creation //

    public bool Hosting(){
        return _runner.IsServer;
    }

    public async void StartGame(GameMode mode, string session_id, int players, string password)
    {
        PlayerPrefs.SetString("Error Details", "Session closed unexpectantly.");
        
        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(GameplaySceneIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid) {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        password_buffer = password;

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = session_id,
            PlayerCount = players,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            SessionProperties = new Dictionary<string, SessionProperty>(){
                {"Owner", PlayerPrefs.GetString("USERNAME")},
                {"Password", password},
                {"Game_Started", false}
            }
        });

        if(BackgroundColours != null)
            BackgroundColours.Save();
    }

    public static string RandomString(int length){
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    // Joining, Leaving, Starting games //

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player){
        if (Hosting()){
            NetworkObject new_player = runner.Spawn(PlayerInstancePrefab, Vector3.zero, Quaternion.identity, player);
            PlayerInstance new_player_instance = new_player.GetComponent<PlayerInstance>();
            
            new_player_instance.SetID(player_counter);
            player_counter++;

            _spawnedCharacters.Add(player, new_player);
            if(player == _runner.LocalPlayer){
                new_player_instance.WeAreHost();
            }
        }
    }

    public NetworkObject SpawnObject(NetworkPrefabRef prefab){
        return _runner.Spawn(prefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player){
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject)){
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList){ 
        if(_LobbyUI != null)
            _LobbyUI.OnSessionListUpdated(sessionList);

        if(_AutoMatchmake != null)
            _AutoMatchmake.GotSessions(sessionList);   
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason){} // Destroys itself before disconnect stuff can be called.
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason){}
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }
}