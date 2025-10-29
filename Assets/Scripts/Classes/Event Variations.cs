using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventUtils{

    public class DebugEvent : WorldEvent{

        public DebugEvent(int turn) : base(turn) { }
        
        public override void Timekeep(int turn, int player){
            if(base.Active(turn, player))
                Functionality();
        }

        public override void Functionality(){
            Debug.Log("Hello World!");
        }
    }

    public class MessageEvent : WorldEvent{

        MessageContents msg_contents;
        MessageManager manager;
        
        public MessageEvent(int trn, int player_id, MessageContents contents, MessageManager manager) : base(trn, player_id){ 
            msg_contents = contents;
        }

        public void Timekeep(int turn, int player){
            if(base.Active(turn, player))
                Functionality();
        }

        public override void Functionality(){
            manager.Display(msg_contents);
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

        string Type(){return type;}
        string Header(){return Format(header);}
        string Body(){return Format(header);}

        string Format(string input){
            if(faction_one != null)
                input = input.Replace("{0}", faction_one.Name());
            if(faction_two != null)
                input = input.Replace("{1}", faction_two.Name());
            return input;
        }
    }
}