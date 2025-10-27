using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventUtils{

    public class WorldEvent{

        public int start_turn {get; private set;}
        public int end_turn {get; private set;}
        bool done_first, done_last;

        // INSTANTIATION //

        public WorldEvent(int start, int end){
            BaseValues();
            CheckFirst();
        }

        void BaseValues(){
            start_turn = start;
            end_turn = end;
            done_first = false;
            done_last = false;
        }

        // TURN CHECKS //

        public void Check(){
            CheckFirst();
            CheckLast();
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

        public bool Active(int current_turn){return start_turn <= current_turn && !Retired();}
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
            foreach(WorldEvent event in ongoing_events)
                event.Check();
        }

        void CleanEvents(){
            if(ongoing_events.Count == 0)
                return;
            List<WorldEvent> cleaned_events = new List<WorldEvent>();
            foreach(WorldEvent event in ongoing_events){
                if(!ongoing_events[i].Retired(current_turn))
                    cleaned_events.Add(ongoing_events[i]);
            }
            ongoing_events = cleaned_events;
        }
    }
}