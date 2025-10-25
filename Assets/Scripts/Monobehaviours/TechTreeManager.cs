using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using HenrysTechUtils;

public class TechTreeManager : MonoBehaviour{

    [SerializeField] TechTreeRoot[] Roots;
    [SerializeField] TechnologyDefinition DefaultTechnologies;
    TechNode[] root_nodes;
    
    Dictionary<TroopData, bool> TroopOwnershipMap = new Dictionary<TroopData, bool>();
    Dictionary<PieceData, bool> BuildingOwnershipMap = new Dictionary<PieceData, bool>();

    // INSTANTIATION //

    public void Setup(){
        PopulateTrees();
        SetDefaults();
    }

    void PopulateTrees(){
        root_nodes = new TechNode[Roots.Length];
        for(int i = 0; i < Roots.Length; i++){
            root_nodes[i] = new TechNode(Roots[i].FirstNode());
            EstablishOwnershipMap(root_nodes[i]);
        }
    }

    void SetDefaults(){
        if(DefaultTechnologies == null)
            return;

        TechNode default_tech = new TechNode(DefaultTechnologies);
        default_tech.Unlock();
        EstablishOwnershipMap(default_tech);
    }

    // OWNERSHIP MAP //

    void EstablishOwnershipMap(TechNode tech){
        FillMap(tech);
        if(tech.HasChildren()){
            foreach(TechNode child in tech.Next()){
                EstablishOwnershipMap(child);
            }
        }
    }

    void FillMap(TechNode tech){
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

    // TROOP / BUILDING VALIDATION //

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

    // SETTERS //

    public void Unlock(TechNode tech){
        tech.Unlock();
        FillMap(tech);
    }

    // GETTERS //

    public string NodeName(int index){
        if(index < 0 || index >= RootObjects().Length)
            return "NULL";
        return RootObjects()[index].Title();
    }

    public TechNode[] RootNodes(){return root_nodes;}
    public TechTreeRoot[] RootObjects(){return Roots;}
}