using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random=System.Random;
using Fusion;
using Fusion.Sockets;

public class AutoMatchmake : MonoBehaviour
{
    [SerializeField] public ConnectionManager _ConnectionManager;
    private static Random random = new Random();

    void Start(){
        _ConnectionManager._AutoMatchmake = this;
        _ConnectionManager.ConnectToLobby();
    }

    public void GotSessions(List<SessionInfo> sessionList){
        string session_name = "";

        for(int i = 0; i < sessionList.Count && session_name == ""; i++){
            if(sessionList[i].Properties["Password"] == "" && !sessionList[i].Properties["Game_Started"] && sessionList[i].PlayerCount < sessionList[i].MaxPlayers){
                session_name = sessionList[i].Name;
            }
        }

        if(session_name == "")
            _ConnectionManager.StartGame(GameMode.Host, RandomString(16), 3, "");
        else
            _ConnectionManager.StartGame(GameMode.Client, session_name, 0, "");
    }

    // Random //
    public static string RandomString(int length){
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
