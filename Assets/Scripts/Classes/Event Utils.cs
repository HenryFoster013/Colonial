using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventUtils{

    public class WorldEvent{

        public int start_turn {get; private set;}
        public int end_turn {get; private set;}
        bool done_first, done_last;

        // INSTANTIATION //

        public WorldEvent(int start, int end, int current_turn){
            start_turn = start;
            end_turn = end;
            done_first = false;
            done_last = false;

            CheckFirst(current_turn);
        }

        // TURN CHECKS //

        public void Check(int current_turn){
            CheckFirst(current_turn);
            CheckLast(current_turn);
        }

        void CheckFirst(int current_turn){
            if(done_first)
                return;
            if(current_turn == start_turn)
                StartEvent();
        }

        void CheckLast(int current_turn){
            if(done_last)
                return;
            if(current_turn == end_turn)
                EndEvent();
        }

        // EXPANDABLES //

        public void StartEvent(){ }

        public void EndEvent(){ }

        // GETTERS //

        public bool Active(int current_turn){return start_turn <= current_turn && !Retired(current_turn);}
        public bool Retired(int current_turn){return (current_turn >= end_turn);}
    }

    public class WorldEventManager{

        List<WorldEvent> ongoing_events = new List<WorldEvent>();
        int current_turn;

        public WorldEventManager(int base_turn){
            current_turn = base_turn;
        }

        // TURN CLOCK //

        public void NewTurn(){
            current_turn++;
            CheckEvents();
            CleanEvents();
        }

        void CheckEvents(){
            if(ongoing_events.Count == 0)
                return;
            foreach(WorldEvent world_event in ongoing_events)
                world_event.Check(current_turn);
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