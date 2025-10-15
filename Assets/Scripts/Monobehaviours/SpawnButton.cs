using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnButton : MonoBehaviour{

    [SerializeField] Image MainBG;
    [SerializeField] Image CostBG;
    [SerializeField] RawImage RenderTextureDisplay;
    [SerializeField]GameObject CantAffordIcon;
    PreviewRenderer pr; 
    
    public void Setup(PreviewRenderer _pr, Color col, bool affordable, RenderTexture tex){
        pr = _pr;
        MainBG.GetComponent<Image>().color = col;
        CostBG.GetComponent<Image>().color = col;
        RenderTextureDisplay.texture = tex;
    }

    public void SetAfford(bool affordable){
        CantAffordIcon.SetActive(!affordable);
    }

    public void Pressed(){
        pr.PressButton();
    }

    public bool IsTroop(){
        return pr.IsTroop();
    }

    public int Reference(){
        return pr.Reference();
    }
}
