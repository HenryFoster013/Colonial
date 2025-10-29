using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static GenericUtils;
using TMPro;
using UnityEngine.UI;

public class NewspaperWindow : Window {

    [Header("Display")]
    [SerializeField] TMP_Text Header;
    [SerializeField] TMP_Text Body;
    [SerializeField] Image BaseFlag;
    [SerializeField] Image OverlayFlag;

    public void Setup(MessageContents message){
        if(!message.CheckType("NEWSPAPER")){
            SilentClose();
            Destroy(this.gameObject);
            return;
        }

        Header.text = message.Header();
        Body.text = message.Body();
        SetupFlags();
        SilentOpen();
    }

    public void SetupFlags(){
        BaseFlag.sprite = message.BaseFlag();
        OverlayFlag.gameObject.SetActive(message.OverlayFlag() != null);
        OverlayFlag.sprite = message.OverlayFlag();
    }
}