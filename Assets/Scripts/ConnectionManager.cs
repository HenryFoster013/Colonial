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
    const string DisconnectScene = "Title Screen";
    [HideInInspector] public LobbyUI _LobbyUI;
    [HideInInspector] public AutoMatchmake _AutoMatchmake;
    
    
    private NetworkRunner _runner;
    private static Random random = new Random();
    bool connected_to_lobby;
    public bool ConnectedToLobby(){return connected_to_lobby;}

    // Lobbies //

    public async void ConnectToLobby(){
        connected_to_lobby = false;
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        RefreshLobbyConnection();
    }

    public async void DisconnectFromLobby(){
        await _runner.Shutdown();
        SceneManager.LoadScene(DisconnectScene);
    }

    public async void RefreshLobbyConnection(){
        connected_to_lobby = false;
        UpdateLobbyUI();
        await _runner.JoinSessionLobby(SessionLobby.Shared);
        connected_to_lobby = true;
        UpdateLobbyUI();
    }

    // Lobby UI Interactions //

    void UpdateLobbyContentSize(){
        if(_LobbyUI != null)
            _LobbyUI.UpdateContentSize();
    }

    void UpdateLobbyUI(){
        if(_LobbyUI != null)
            _LobbyUI.UpdateLobbyUI();
    }

    // Session Hosting and Creation //

    public async void StartGame(GameMode mode, string session_id, int players, string password)
    {
        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(GameplaySceneIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid) {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

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
                {"Password", password}
            }
        });
    }

    public static string RandomString(int length){
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    // Joining, Leaving, Starting games //

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1, 0);
            NetworkObject networkPlayerObject = runner.Spawn(PlayerInstancePrefab, spawnPosition, Quaternion.identity, player);
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
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

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){ }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }
}