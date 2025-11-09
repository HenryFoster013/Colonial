using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static GenericUtils;

public class PreGameManager : MonoBehaviour
{
    [Header(" - References - ")]
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] SessionManager _SessionManager;
    [SerializeField] SoundEffectLookup SFX_Lookup;
    
    [Header(" - UI - ")]
    [SerializeField] GameObject UI_Holder;
    [SerializeField] GameObject InteractableButtonsHolder;
    [SerializeField] GameObject HostOnly_UI;
    [SerializeField] GameObject StartButton;
    [SerializeField] TMP_Text CantStartText;
    [SerializeField] Image Flag;

    [Header("UI - Player Icons")]
    [SerializeField] RectTransform[] PlayerIcons;
    [SerializeField] TMP_Text[] PI_Usernames;
    [SerializeField] Image[] PI_Backgrounds;
    [SerializeField] Image[] PI_Flags;
    [SerializeField] MeshRenderer[] PI_TorsoMeshes;
    [SerializeField] GameObject[] PI_HostStars;
    [Header("Meshes")]
    [SerializeField] PregameTroopDisplay[] PI_Troops;
    
    public void Setup(){
        UI_Holder.SetActive(true);
        HostOnly_UI.SetActive(_SessionManager.Hosting);
        UpdateFlag();
    }

    public void UpdateUI(){
        int player_count = _SessionManager.GetPlayerCount();
        float icon_dist = 185;
        float offset = -0.5f * icon_dist * (player_count - 1);

        for(int i = 0; i < PlayerIcons.Length; i++){
            bool valid = false;
            if(i < player_count){
                PlayerInstance playa = _SessionManager.GetPlayer(i); 
                valid = (playa.Username != "");
                if(valid){
                    PlayerIcons[i].anchoredPosition = new Vector2((icon_dist * i) + offset, PlayerIcons[i].anchoredPosition.y);
                    PI_Usernames[i].text = playa.GetUsername();
                    PI_Backgrounds[i].color = playa.FactionData().Colour();
                    PI_Flags[i].sprite = playa.FactionData().Mini_Flag();
                    PI_HostStars[i].SetActive(playa.Host);
                    PI_Troops[i].SetTroop(playa.Faction_ID);
                }
            }
            PlayerIcons[i].gameObject.SetActive(valid);
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
        PlaySFX("UI_2", SFX_Lookup);
    }

    public void CloseUI_NoSFX(){
        UI_Holder.SetActive(false);
    }

    public void SetStartButtons(bool not_waiting, bool all_unique, bool all_ready){
        CantStartText.gameObject.SetActive(!not_waiting || !all_unique);
        CantStartText.text = "(waiting for players)";
        if(not_waiting)
            CantStartText.text = "(all players must be different factions)";
        StartButton.SetActive(not_waiting && all_unique && all_ready);
    }

    public void ChangeFaction(int modifier){
        if(_SessionManager.game_state != 0)
            return;

        int new_faction = PlayerPrefs.GetInt("FACTION") + modifier;
        if(new_faction < 0)
            new_faction = _FactionLookup.Length() - 1;
        if(new_faction >= _FactionLookup.Length())
            new_faction = 0;
        PlayerPrefs.SetInt("FACTION", new_faction);

        _SessionManager.OurInstance.UpdateFaction();
        PlaySFX("UI_1", SFX_Lookup);
        UpdateFlag();
    }
}
