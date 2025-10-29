using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventUtils{
    
    public class DebugEvent : WorldEvent{

        public DebugEvent(int turn) : base(turn) { }
        
        public override void Timekeep(int turn, int player){
            if(base.Active(turn, player))
                Functionality();
        }

        public override void Functionality(){
            Debug.Log("Debug event! Uhm- that just happened!");
        }
    }

    public class TruceEvent : WorldEvent{

        Faction faction_one;
        Faction faction_two;

        public TruceEvent(Faction fac_one, Faction fac_two, int start_turn, int duration) : base(start_turn, start_turn + duration, -1){
            faction_one = fac_one;
            faction_two = fac_two;
        }

        public bool CheckTruce(Faction fac_one, Faction fac_two, int current_turn, int current_player){
            bool correct_factions = (faction_one == fac_one && faction_two == fac_two) || (faction_one == fac_two && faction_two == fac_one);
            return correct_factions && Active(current_turn, current_player);
        }
    }
}