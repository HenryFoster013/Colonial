using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using HenrysTechUtils;

public class TechTreeManager : MonoBehaviour{

    [SerializeField] TechTreeRoot[] Roots;
    [SerializeField] TechnologyDefinition DefaultTechnologies;
    TechNode[] start_nodes;
    
    Dictionary<TroopData, bool> TroopOwnershipMap = new Dictionary<TroopData, bool>();
    Dictionary<PieceData, bool> BuildingOwnershipMap = new Dictionary<PieceData, bool>();

    // Initialisation

    public void Setup(){
        PopulateTrees();
        SetDefaults();
    }

    void PopulateTrees(){
        start_nodes = new TechNode[Roots.Length];
        for(int i = 0; i < Roots.Length; i++){
            start_nodes[i] = new TechNode(Roots[i].FirstNode());
            EstablishOwnershipMap(start_nodes[i]);
        }
    }

    void SetDefaults(){
        if(DefaultTechnologies == null)
            return;

        TechNode default_tech = new TechNode(DefaultTechnologies);
        default_tech.Unlock();
        EstablishOwnershipMap(default_tech);
    }

    // Ownership population

    int counter = 0;

    void EstablishOwnershipMap(TechNode tech){
        
        SetUnlocks(tech);

        if(tech.HasChildren()){
            foreach(TechNode child in tech.Next()){
                EstablishOwnershipMap(child);
            }
        }
    }

    void SetUnlocks(TechNode tech){
        if(tech.HasTroops()){
            foreach(TroopData troop in tech.Troops()){
                if(!TroopOwnershipMap.ContainsKey(troop))
                    TroopOwnershipMap.Add(troop, tech.unlocked);
                else
                    TroopOwnershipMap[troop] = tech.unlocked;
            }
        }
        if(tech.HasBuildings()){
            foreach(PieceData building in tech.Buildings()){
                if(!BuildingOwnershipMap.ContainsKey(building))
                    BuildingOwnershipMap.Add(building, tech.unlocked);
                else
                    BuildingOwnershipMap[building] = tech.unlocked;
            }
        }
    }

    // Troop/Building validation

    public bool Unlocked(TroopData troop){
        if(!TroopOwnershipMap.ContainsKey(troop))
            return false;
        
        return (TroopOwnershipMap[troop]);
    }

    public bool Unlocked(PieceData building){
        if(!BuildingOwnershipMap.ContainsKey(building))
            return false;
        
        return (BuildingOwnershipMap[building]);
    }

    // Unlocking

    public void Unlock(TechNode tech){
        tech.Unlock();
        SetUnlocks(tech);
    }

    // Getters

    public TechNode[] GetRootNodes(){return start_nodes;}

}