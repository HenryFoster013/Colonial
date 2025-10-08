using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnButton : MonoBehaviour{

    [SerializeField] Image MainBG;
    [SerializeField] Image CostBG;
    [SerializeField] TMP_Text CostText;
    [SerializeField] RawImage RenderTextureDisplay;
    PreviewRenderer pr; 
    
    public void Setup(PreviewRenderer _pr, Color col, int cost, RenderTexture tex){
        pr = _pr;
        MainBG.GetComponent<Image>().color = col;
        CostBG.GetComponent<Image>().color = col;
        CostText.text = cost.ToString();
        RenderTextureDisplay.texture = tex;
    }

    public void Pressed(){
        pr.PressButton();
    }
}
