using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TruceUtils{

    public class TruceManager{

        public TruceManager() { }

        Dictionary<Faction, List<Faction>> truce_lookup = new Dictionary<Faction, List<Faction>>();

        public void NewTruce(Faction fac_one, Faction fac_two){
            if(Truced(fac_one, fac_two))
                return;
            
            CheckEmpty(fac_one);
            CheckEmpty(fac_two);
            
            if(!truce_lookup[fac_one].Contains(fac_two))
                truce_lookup[fac_one].Add(fac_two);
            if(!truce_lookup[fac_two].Contains(fac_one))
                truce_lookup[fac_two].Add(fac_one);
        }

        public void EndTruce(Faction fac_one, Faction fac_two){
            if(!Truced(fac_one, fac_two))
                return;
            
            CheckEmpty(fac_one);
            CheckEmpty(fac_two);

            if(truce_lookup[fac_one].Contains(fac_two))
                truce_lookup[fac_one].Remove(fac_two);
            if(truce_lookup[fac_two].Contains(fac_one))
                truce_lookup[fac_two].Remove(fac_one);
        }

        public bool Truced(Faction fac_one, Faction fac_two){
            List<Faction> truced = truce_lookup[fac_one];
            if(truced.Count == 0)
                return false;

            foreach(Faction fac in truced){
                if(fac == fac_two)
                    return true;
            }

            return false;
        }

        void CheckEmpty(Faction faction){
            if(!truce_lookup.ContainsKey(faction)){
                truce_lookup.Add(faction, new List<Faction>());
            }
        }
    }
}