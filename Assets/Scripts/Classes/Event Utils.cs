using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventUtils{

    // BASE CLASSES //

    public class WorldEvent{

        int activation_turn;
        int target_player; // -1 for "I don't care", not who can see it but the sub turn it occurs on        
        public EventManager outside_manager;
        
        // Instantiation
        public void SetEventManager(EventManager messanger){outside_manager = messanger;}
        public WorldEvent(int turn) : this(turn, -1) { }
        public WorldEvent(int start, int target){
            activation_turn = start;
            target_player = target;
        }

        // Clock
        public bool Active(int current_turn, int current_player){return current_turn == activation_turn && (target_player == -1 || target_player == current_player);}
        public bool Retired(int current_turn){return (current_turn > activation_turn);}
        
        // Expandables
        public virtual void Functionality(){ }
    }

    public class WorldEventManager{

        List<WorldEvent> ongoing_events = new List<WorldEvent>();
        int current_turn;
        int current_player;

        // These are the references to monobehaviours used to render events in the scene
        // Eg, out_manag manager for popups and other managers for things like tornados or whatever
        EventManager outside_manager;

        public WorldEventManager(EventManager out_managr){ 
            outside_manager = out_managr;
        }

        public void Tick(int turn, int player){
            current_turn = turn;
            current_player = player;
            RunEvents();
        }

        public void Add(WorldEvent new_event){
            ongoing_events.Add(new_event);
            new_event.SetEventManager(outside_manager);
            Tick(current_turn, current_player); // Used for immediate events (such as accepting peace)
        }

        //  Management //

        void RunEvents(){

            if(ongoing_events.Count == 0)
                return;

            List<WorldEvent> cleaned_events = new List<WorldEvent>();
            
            foreach(WorldEvent world_event in ongoing_events){
                if(world_event.Active(current_turn, current_player))
                    world_event.Functionality();
                else{
                    if(!world_event.Retired(current_turn))
                        cleaned_events.Add(world_event);
                }
            }

            ongoing_events = cleaned_events;
        }
    }
}