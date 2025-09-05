using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Troop Lookup", menuName = "Custom/Troops/Lookup")]
public class TroopLookup : ScriptableObject
{
    [SerializeField] TroopData[] Troops;

    public TroopData Troop(int i){
        return Troops[i];
    }   

    public int ID(TroopData reference){
        int return_val = 0;
        bool done = false;
        for(int i = 0; i < Troops.Length && !done; i++){
            if(Troops[i] == reference){
                done = true;
                return_val = i;
            }
        }
        return return_val;
    }
}
