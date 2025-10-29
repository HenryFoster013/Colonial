using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventUtils{

    // BASE CLASSES //

    public class WorldEvent{

        string type = "NULL";
        int activation_turn;
        int end_turn;
        int target_player; // -1 == all (immediate)

        public EventManager outside_manager;
        
        // Instantiation //
        public WorldEvent(int start, int end, int target){
            activation_turn = start;
            end_turn = end;
            target_player = target;
        }

        public WorldEvent(int turn, int target) : this(turn, turn + 1, target) { }
        public WorldEvent(int turn) : this(turn, turn + 1, -1) { }

        // Expandables
        public virtual void Functionality(){ }
        public virtual void Timekeep(int current_turn, int current_player) { }

        // Setters
        public void SetType(string _type){type = _type;}
        public void SetEventManager(EventManager messanger){outside_manager = messanger;}

        // Getters
        public bool Active(int current_turn, int current_player){return !Retired(current_turn) && (target_player == -1 || target_player == current_player);}
        public bool Retired(int current_turn){return (current_turn < activation_turn || current_turn >= end_turn);}
        public bool CheckType(string input){return (type.ToUpper() == input.ToUpper());}
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
            CheckEvents();
            CleanEvents();
        }

        public void Add(WorldEvent new_event){
            ongoing_events.Add(new_event);
            new_event.SetEventManager(outside_manager);
        }

        //  Management //

        void CheckEvents(){
            if(ongoing_events.Count == 0)
                return;
            foreach(WorldEvent world_event in ongoing_events)
                world_event.Timekeep(current_turn, current_player);
        }

        void CleanEvents(){
            if(ongoing_events.Count == 0)
                return;
            List<WorldEvent> cleaned_events = new List<WorldEvent>();
            foreach(WorldEvent world_event in ongoing_events){
                if(world_event.Retired(current_turn))
                    cleaned_events.Add(world_event);
            }
            ongoing_events = cleaned_events;
        }
    }
}