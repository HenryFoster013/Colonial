using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventUtils{
    
    public class DebugEvent : WorldEvent{

        public DebugEvent(int turn) : base(turn) { }

        public override void Functionality(){
            Debug.Log("Debug event! Uhm- that just happened!");
        }
    }
}