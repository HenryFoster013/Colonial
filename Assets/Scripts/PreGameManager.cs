using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreGameManager : MonoBehaviour
{
    [Header(" - References - ")]
    [SerializeField] FactionLookup _FactionLookup;
    SessionManager _SessionManager;
    
    [Header(" - UI - ")]
    [SerializeField] GameObject UI_Holder;
    [SerializeField] GameObject InteractableButtonsHolder;
    [SerializeField] GameObject HostOnly_UI;
    [SerializeField] GameObject StartButton;
    [SerializeField] GameObject CantStartText;
    [SerializeField] Image Flag;

    [Header("UI - Player Icons")]
    [SerializeField] RectTransform[] PlayerIcons;
    [SerializeField] TMP_Text[] PI_Usernames;
    [SerializeField] Image[] PI_Backgrounds;
    [SerializeField] Image[] PI_Flags;
    [SerializeField] MeshRenderer[] PI_TorsoMeshes;
    [SerializeField] GameObject[] PI_HostStars;
    
    public void Setup(SessionManager sm){
        _SessionManager = sm;
        UI_Holder.SetActive(true);
        HostOnly_UI.SetActive(_SessionManager.Hosting);
        UpdateFlag();
    }

    public void UpdateUI(){
        int player_count = _SessionManager.GetPlayerCount();
        float icon_dist = 185;
        float offset = -0.5f * icon_dist * (player_count - 1);

        for(int i = 0; i < PlayerIcons.Length; i++){
            PlayerIcons[i].gameObject.SetActive(i < player_count);
            if(i < player_count){
                PlayerInstance playa = _SessionManager.GetPlayer(i);

                PlayerIcons[i].anchoredPosition = new Vector2((icon_dist * i) + offset, PlayerIcons[i].anchoredPosition.y);
                PI_Usernames[i].text = playa.Username;
                PI_Backgrounds[i].color = playa.FactionData().Colour();
                PI_Flags[i].sprite = playa.FactionData().Mini_Flag();
                PI_TorsoMeshes[i].SetPropertyBlock(_SessionManager.GetTroopMaterials(playa.Faction_ID)[0]);
                PI_HostStars[i].SetActive(playa.Host);
            }
        }

        if(_SessionManager.OurInstance == null)
            InteractableButtonsHolder.SetActive(false);
        else
            InteractableButtonsHolder.SetActive(_SessionManager.OurInstance.Ready);
    }

    public void UpdateFlag(){
        Flag.sprite = _FactionLookup.GetFaction(PlayerPrefs.GetInt("FACTION")).Flag();
    }

    public void CloseUI(){
        UI_Holder.SetActive(false);
    }

    public void SetStartButtons(bool can_war, bool all_ready){
        CantStartText.SetActive(!can_war || !all_ready);
        StartButton.SetActive(can_war && all_ready);
    }
}
