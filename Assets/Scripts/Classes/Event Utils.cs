using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventUtils{

    // BASE CLASSES //

    public class WorldEvent{

        public EventManager outside_manager;
        
        // Instantiation
        public void SetEventManager(EventManager messanger){outside_manager = messanger;}
        public WorldEvent(){ }
        
        // Expandables
        public virtual void Functionality(){ }
    }

    public class WorldEventManager{

        List<WorldEvent> ongoing_events = new List<WorldEvent>();

        // These are the references to monobehaviours used to render events in the scene
        // Eg, out_manag manager for popups and other managers for things like tornados or whatever
        EventManager outside_manager;

        public WorldEventManager(EventManager out_managr){ 
            outside_manager = out_managr;
        }

        public void Add(WorldEvent new_event){
            ongoing_events.Add(new_event);
            new_event.SetEventManager(outside_manager);
        }

        //  Management //

        public void RunEvents(){

            if(ongoing_events.Count == 0)
                return;
            foreach(WorldEvent world_event in ongoing_events){
                world_event.Functionality();
            }
            ongoing_events = new List<WorldEvent>();
        }
    }

    // VARIANTS //

    public class DebugEvent : WorldEvent{
        public DebugEvent() : base() { }

        public override void Functionality(){
            Debug.Log("Debug event! Uhm- that just happened!");
        }
    }
}