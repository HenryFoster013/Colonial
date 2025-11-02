using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventUtils{

    public class MessageEvent : WorldEvent{

        MessageContents msg_contents;
        
        public MessageEvent(MessageContents contents){ 
            msg_contents = contents;
        }
        
        public override void Functionality(){
            outside_manager.Message(msg_contents);
        }
    }

    public class MessageContents{

        string type;
        
        Faction faction_one;
        Faction faction_two;

        string header;
        string body;

        public MessageContents(string _type, string _header, string _body, Faction fac_one, Faction fac_two){
            type = _type;
            header = _header;
            body = _body;
            faction_one = fac_one;
            faction_two = fac_two;
        }

        public string Type(){return type.ToUpper();}
        public bool CheckType(string input){return Type() == input.ToUpper();}
        public string Header(){return Format(header);}
        public string Body(){return Format(body);}
        public Faction FactionOne(){return faction_one;}
        public Faction FactionTwo(){return faction_two;}
        public Sprite BaseFlag(){return GetFlag(faction_one);}
        public Sprite OverlayFlag(){return GetFlag(faction_two);}

        Sprite GetFlag(Faction fac){
            if(fac == null)
                return null;
            return fac.Flag();
        }

        public string Format(string input){
            if(faction_one != null)
                input = input.Replace("{0}", faction_one.Name());
            if(faction_two != null)
                input = input.Replace("{1}", faction_two.Name());
            return input;
        }
    }
}