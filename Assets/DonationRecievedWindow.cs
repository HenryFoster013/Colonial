using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenericUtils;
using TMPro;

public class DonationRecievedWindow : Window
{
    [Header("Donation Specifics")]
    [SerializeField] TMP_Text BodyText;
    [SerializeField] TMP_Text ButtonText;
    
    SessionManager _SessionManager;
    int cash;
    
    public void Setup(Faction donor, string amount, SessionManager manager){

        print("nigga");
        
        cash = -1;
        cash = int.Parse(amount);

        if(cash < 0 || cash > 999){
            SilentClose();
            return;
        }

        print("Vagina");
        print(cash);

        _SessionManager = manager;
        BodyText.text = "The " + donor.Name() + " have donated " + donor.CurrencyFormat(amount) + "!";
        ButtonText.text = donor.CurrencyFormat(amount);
    }

    public override void Close(){
        print("bye");
        SilentClose();
        PlaySFX(CloseSFX);
        _SessionManager.EarnMoney(cash);
    }
}
