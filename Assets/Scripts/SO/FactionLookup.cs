using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Faction Lookup", menuName = "Custom/Factions/Lookup")]
public class FactionLookup : ScriptableObject
{
    [SerializeField] Faction[] _Factions;

    public Faction[] GetFactions(){return _Factions;}

    public Faction GetFaction(int i){
        return _Factions[i];
    }   

    public int Length(){
        return _Factions.Length;
    }

    public int ID(string name){
        int return_val = -1;
        bool done = false;
        for(int i = 0; i < _Factions.Length && !done; i++){
            if(_Factions[i].CheckType(name)){
                done = true;
                return_val = i;
            }
        }

        return return_val;
    }

    public int ID(Faction fac){
        int return_val = -1;
        bool done = false;
        for(int i = 0; i < _Factions.Length && !done; i++){
            if(_Factions[i] == fac){
                done = true;
                return_val = i;
            }
        }

        return return_val;
    }
}
