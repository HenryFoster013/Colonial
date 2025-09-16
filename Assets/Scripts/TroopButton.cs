using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TroopButton : MonoBehaviour{

    [SerializeField] RawImage Display;
    [SerializeField] Image MainBG;
    [SerializeField] Image CostBG;
    [SerializeField] TMP_Text CostText;
    int troop_id;
    PlayerManager pm; 
    
    public void Setup(int count, PlayerManager player_manag, Color col, int cost, RenderTexture render_texture){
        troop_id = count;
        pm = player_manag;

        Display.texture = render_texture;
        MainBG.GetComponent<Image>().color = col;
        CostBG.GetComponent<Image>().color = col;
        CostText.text = cost.ToString();
    }

    public void Pressed(){
        pm.SpawnTroopButton(troop_id);
    }
}
