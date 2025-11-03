using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static GenericUtils;
using TMPro;
using UnityEngine.UI;
using EventUtils;

public class NewspaperWindow : Window {

    [Header("Display")]
    [SerializeField] TMP_Text Header;
    [SerializeField] TMP_Text Body;
    [SerializeField] Image BaseFlag;
    [SerializeField] Image OverlayFlag;
    [Header("Texts")]
    [TextArea] public string PeaceText;
    [TextArea] public string WarText;


    public void Setup(MessageContents message){
        if(!message.CheckType("NEWSPAPER")){
            SilentClose();
            Destroy(this.gameObject);
            return;
        }

        if(message.Body() == "WAR")
            Body.text = message.Format(WarText);
        else
            Body.text = message.Format(PeaceText);

        Header.text = message.Header();
        SetupFlags(message);
        SilentOpen();
        PlaySFX(OpenSFX);
    }

    public void SetupFlags(MessageContents message){
        BaseFlag.sprite = message.BaseFlag();
        OverlayFlag.gameObject.SetActive(message.OverlayFlag() != null);
        OverlayFlag.sprite = message.OverlayFlag();
    }
}