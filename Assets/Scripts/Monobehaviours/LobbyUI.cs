using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random=System.Random;
using UnityEngine.UI;
using TMPro;
using static HenrysUtils;

public class LobbyUI : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] ConnectionManager _ConnectionManager;
    [SerializeField] SoundEffectLookup SFX_Lookup;

    [Header("UI")]
    [SerializeField] GameObject Gen_LoadingIcon;

    [Header("Match Creator")]
    [SerializeField] GameObject MatchCreatorMenu;
    [SerializeField] TMP_Text MCM_Header;
    [SerializeField] TMP_Text MCM_PlayerCount;
    [SerializeField] TMP_InputField MCM_PasswordField;

    [Header("Lobby List")]
    [SerializeField] GameObject LobbyListMenu;
    [SerializeField] GameObject LL_EmptyListPrefab;
    [SerializeField] GameObject LL_ListPrefab;
    [SerializeField] RectTransform LL_ScrollContentRect;
    [SerializeField] GameObject[] LL_EnabledWhenLobbiesLoad;
    [SerializeField] GameObject LL_LoadingIcon;

    [Header("Enter Password")]
    [SerializeField] GameObject EnterPasswordMenu;
    [SerializeField] TMP_InputField EP_PasswordField;
    [SerializeField] TMP_Text EP_Header;
    [SerializeField] GameObject EP_Main;
    [SerializeField] GameObject EP_Incorrect;
    
    private static Random random = new Random();
    bool mcm_active, ep_active;
    int mcm_player_count = 3;
    string ep_session_name, ep_password, ep_user_name;

    // UI //

    void Start(){
        _ConnectionManager._LobbyUI = this;
        _ConnectionManager.ConnectToLobby();
        GeneralUISetup();
    }

    void GeneralUISetup(){
        mcm_active = false;
        ep_active = false;
        EP_Incorrect.SetActive(false);
        EP_Main.SetActive(true);
        LobbyListMenu.SetActive(true);
        Gen_LoadingIcon.SetActive(false);
        MatchCreatorMenu.SetActive(false);
        EnterPasswordMenu.SetActive(false);
    }

    void Update(){
        CheckConnection();
    }

    void CheckConnection(){
        if(_ConnectionManager == null)
            SceneManager.LoadScene("Network Error");
    }

    // Connection Interactions //

    public void JoinGame(string session_id, string password, string name){
        if(password != ""){
            ep_session_name = session_id;
            ep_password = password;
            ep_user_name = name;
            ep_active = false;
            ToggleEP();
        }
        else{
            LoadingScreen();
            PlaySFX("UI_2", SFX_Lookup);
            _ConnectionManager.StartGame(GameMode.Client, session_id, 0, "");
        }
    }

    public void NewSession(){
        LoadingScreen();
        PlaySFX("UI_2", SFX_Lookup);
        _ConnectionManager.StartGame(GameMode.Host, RandomString(16), mcm_player_count, MCM_PasswordField.text);
    }

    // Lobby List Menu //

    void LoadingScreen(){
        Gen_LoadingIcon.SetActive(true);
        LobbyListMenu.SetActive(false);
        MatchCreatorMenu.SetActive(false);
        EnterPasswordMenu.SetActive(false);
    }

    public void UpdateContentSize(){
        Vector2 size = LL_ScrollContentRect.sizeDelta;
        size.y = 55f * LL_ScrollContentRect.transform.childCount;
        LL_ScrollContentRect.sizeDelta = size;
    }

    public void UpdateLobbyUI(){
        LL_LoadingIcon.SetActive(!_ConnectionManager.connected_to_lobby);
        foreach(GameObject g in LL_EnabledWhenLobbiesLoad){
            g.SetActive(_ConnectionManager.connected_to_lobby);
        }
    }

    public void OnSessionListUpdated(List<SessionInfo> sessionList){ 
        PlaySFX("UI_2", SFX_Lookup);

        foreach(Transform t in LL_ScrollContentRect.transform){
            Destroy(t.gameObject);
        }

        int valid_sessions = 0;
        foreach(var session in sessionList){
            if(!session.Properties["Game_Started"]){
                valid_sessions++;
                GameObject new_list_prefab = GameObject.Instantiate(LL_ListPrefab);
                new_list_prefab.transform.SetParent(LL_ScrollContentRect.transform);
                new_list_prefab.transform.localScale = new Vector3(1,1,1);
                new_list_prefab.GetComponent<LobbyOption>().Setup(this, session.Name, session.Properties["Owner"], session.PlayerCount, session.MaxPlayers, session.Properties["Password"]);
            }
        }

        if(valid_sessions == 0){   
            GameObject g = GameObject.Instantiate(LL_EmptyListPrefab);
            g.transform.SetParent(LL_ScrollContentRect.transform);
            g.transform.localScale = new Vector3(1,1,1);
        }

        Vector2 size = LL_ScrollContentRect.sizeDelta;
        size.y = 55f * LL_ScrollContentRect.transform.childCount;
        LL_ScrollContentRect.sizeDelta = size;
    }

    // Enter Password Menu //

    public void ToggleEP(){
        ep_active = !ep_active;
        EnterPasswordMenu.SetActive(ep_active);
        LobbyListMenu.SetActive(!ep_active);
        Gen_LoadingIcon.SetActive(false);
        MatchCreatorMenu.SetActive(false);
        if(ep_active){
            PlaySFX("UI_1", SFX_Lookup);
            EP_PasswordField.text = "";
            EP_Header.text = ep_user_name + "'s Match";
            EP_Incorrect.SetActive(false);
            EP_Main.SetActive(true);
        }
        else{
            PlaySFX("UI_2", SFX_Lookup);
        }
    }

    public void EnterPassword(){
        if(EP_PasswordField.text == ep_password){
            PlaySFX("UI_2", SFX_Lookup);
            LoadingScreen();
            _ConnectionManager.StartGame(GameMode.Client, ep_session_name, 0, ep_password);
        }
        else{
            PlaySFX("UI_Error", SFX_Lookup);
            EP_Incorrect.SetActive(true);
            EP_Main.SetActive(false);
        }
    }

    // Create Match Menu //

    public void ToggleMCM(){
        mcm_active = !mcm_active;
        MatchCreatorMenu.SetActive(mcm_active);
        Gen_LoadingIcon.SetActive(false);
        LobbyListMenu.SetActive(!mcm_active);
        EnterPasswordMenu.SetActive(false);
        if(mcm_active){
            PlaySFX("UI_1", SFX_Lookup);
            MCM_Header.text = PlayerPrefs.GetString("USERNAME") + "'s Match";
            mcm_player_count = 3;
            MCM_PlayerCount.text = mcm_player_count.ToString();
            MCM_PasswordField.text = "";
        }
        else{
            PlaySFX("UI_2", SFX_Lookup);
        }
    }

    public void AddPlayersMCM(int i){
        mcm_player_count += i;
        if(mcm_player_count < 2)
            mcm_player_count = 2;
        if(mcm_player_count > 6)
            mcm_player_count = 6;
        MCM_PlayerCount.text = mcm_player_count.ToString();
    }

    public void BackToTitle(){
        PlaySFX("UI_2", SFX_Lookup);
        LoadingScreen();
        _ConnectionManager.DisconnectFromLobby("Title Screen");
    }

    // Random //

    public static string RandomString(int length){
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}