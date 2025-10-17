using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static HenrysUtils;

public class FortressUpgradeWindow : Window{

    [Header("Upgrade Variables")]
    [SerializeField] TMP_Text Confirm_Text;
    PlayerManager player;

    public void Setup(string confirm, PlayerManager playa){
        player = playa;
        Confirm_Text.text = confirm;
    }

    public void CashIn(){
        SilentClose();
        PlaySFX("UI_Raise", SFX_Lookup);
    }
}
