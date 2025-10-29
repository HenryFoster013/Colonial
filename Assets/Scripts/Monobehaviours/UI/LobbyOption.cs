using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyOption : MonoBehaviour
{
    [SerializeField] TMP_Text LobbyLabel;
    [SerializeField] TMP_Text PlayerCount;
    [SerializeField] GameObject Padlock;

    LobbyUI lobby_manager;
    string lobby_name;
    string password;
    string username;

    public void Setup(LobbyUI lm, string ln, string ow, int cp, int mp, string pw){
        lobby_manager = lm;
        lobby_name = ln;
        username = ow;
        LobbyLabel.text = ow;
        PlayerCount.text = cp.ToString() + "/" + mp.ToString();
        this.transform.localScale = new Vector3(1,1,1);
        Padlock.SetActive(pw != "");
        password = pw;
    }

    public void JoinPressed(){
        lobby_manager.JoinGame(lobby_name, password, username);
    }
}
