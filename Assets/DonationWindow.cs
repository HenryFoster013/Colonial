using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static GenericUtils;

public class DonationWindow : Window{

    [Header("Donation Uniques")]
    [SerializeField] TMP_InputField MoneyInput;
    [SerializeField] GameObject CorrectButton;
    [SerializeField] GameObject IncorrectButton;
    [SerializeField] SessionManager _SessionManager;
    [SerializeField] PlayerCommunicationManager _PlayerCommunicationManager;
    [SerializeField] SoundEffect PurchaseSFX;
    Faction faction;

    public override void Open(){
        MoneyInput.text = "";
        RefreshUI();
        SilentOpen();
        PlaySFX(OpenSFX);
    }

    public void SetFaction(Faction fact){faction = fact;}

    void RefreshUI(){
        bool valid = FieldToInt() != -1;
        CorrectButton.SetActive(valid);
        IncorrectButton.SetActive(valid);
    }

    public void ValidButton(){
        int value = FieldToInt();
        if(value == -1){
            InvalidButton();
            return;
        }

        PlaySFX(PurchaseSFX);
        _PlayerCommunicationManager.DonateFunds(faction, value);
    }

    public void InvalidButton(){
        PlaySFX("UI_4", SFX_Lookup);
    }

    int FieldToInt(){
        int result = -1;
        if (int.TryParse(MoneyInput.text, out result)){
            if(result > 0 && result < 1000 && result <= _SessionManager.Money()){
                return result;
            }
            else
                return -1;
        }
        else
            return -1;
    }

}
