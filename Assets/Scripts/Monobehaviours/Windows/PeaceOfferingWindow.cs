using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static GenericUtils;
using EventUtils;

public class PeaceOfferingWindow : Window
{
    [Header("Treaty Specifics")]
    [SerializeField] TMP_Text Body;
    [TextArea] public string BaseMessage;
    [SerializeField] SoundEffect AcceptSFX;

    Faction faction_one;
    Faction faction_two;
    string our_name;
    string their_name;
    GameplayManager _GamePlayManager;

    public void Setup(GameplayManager manager, MessageContents message){
        _GamePlayManager = manager;
        faction_one = message.FactionOne();
        faction_two = message.FactionTwo();
        our_name = _GamePlayManager.LocalUsername();
        their_name = message.Header();

        Body.text = FormatMessage(BaseMessage);
    }

    string FormatMessage(string msg){
        msg = msg.Replace("{0}", our_name);
        msg = msg.Replace("{1}", faction_one.Name().ToUpper());
        msg = msg.Replace("{2}", their_name);
        return msg;
    }

    public void Accept(){
        _GamePlayManager.MakePeace(faction_one, faction_two);
        PlaySFX(AcceptSFX);
        Destroy(this.gameObject);
    }

    public void TestWar(Faction fact_main, Faction fact_targ){
        if((fact_main == faction_one && fact_targ == faction_two) || (fact_targ == faction_one && fact_main == faction_two)){
            Close();
        }
    }
}
