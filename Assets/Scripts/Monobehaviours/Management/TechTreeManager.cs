using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TechUtils;
using static GenericUtils;

public class TechTreeManager : MonoBehaviour{

    [Header("Technologies")]
    [SerializeField] TechTreeRoot[] Roots;
    [SerializeField] TechnologyDefinition DefaultTechnologies;
    [Header("UI")]
    [SerializeField] TechTreeUI UI;
    [Header("References")]
    [SerializeField] PlayerManager _PlayerManager;
    [SerializeField] SoundEffectLookup SFX_Lookup;

    TechNode[] root_nodes;
    AbstractTechManager abstract_manager;
    Faction faction;
    Dictionary<TroopData, TechNode> TroopOwnershipMap = new Dictionary<TroopData, TechNode>();
    Dictionary<PieceData, TechNode> BuildingOwnershipMap = new Dictionary<PieceData, TechNode>();

    // INTERACTION //

    public void OpenUI(){
        PlaySFX("UI_Raise", SFX_Lookup);
        UI.gameObject.SetActive(true);
        UI.Check();
    }

    public void CloseUI(){
        PlaySFX("UI_1", SFX_Lookup);
        UI.gameObject.SetActive(false);
    }

    // INSTANTIATION //

    public void Setup(Faction our_faction){
        abstract_manager = new AbstractTechManager();
        PopulateTrees();
        SetDefaults();
        faction = our_faction;
        UI.gameObject.SetActive(false);
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
                    TroopOwnershipMap.Add(troop, tech);
            }
        }
        if(tech.HasBuildings()){
            foreach(PieceData building in tech.Buildings()){
                if(!BuildingOwnershipMap.ContainsKey(building))
                    BuildingOwnershipMap.Add(building, tech);
            }
        }
    }

    // TROOP / BUILDING / ABSTRACT VALIDATION //

    public bool Unlocked(TroopData troop){
        if(!TroopOwnershipMap.ContainsKey(troop))
            return false;
        return (TroopOwnershipMap[troop].unlocked);
    }

    public bool Unlocked(PieceData building){
        if(!BuildingOwnershipMap.ContainsKey(building))
            return false;
        return (BuildingOwnershipMap[building].unlocked);
    }

    public bool Unlocked(string abstr){
        return abstract_manager.Unlocked(abstr);
    }

    public TechNode ParentNode(TroopData troop){return TroopOwnershipMap[troop];}
    public TechNode ParentNode(PieceData piece){return BuildingOwnershipMap[piece];}

    // SETTERS //

    public void Unlock(TechNode tech){tech.Unlock();}
    public void Unlock(string abstr){abstract_manager.Unlock(abstr);}
    public void SpendMoney(int cost){_PlayerManager.SpendMoney(cost);}

    // GETTERS //

    public string NodeName(int index){
        if(index < 0 || index >= RootObjects().Length)
            return "NULL";
        return RootObjects()[index].Title();
    }

    public bool CanAfford(int cost){return (cost <= _PlayerManager.Money());}
    public string FormatMoney(int money){return faction.CurrencyFormat(money);}
    public TechNode[] RootNodes(){return root_nodes;}
    public TechTreeRoot[] RootObjects(){return Roots;}
    public int Money(){return _PlayerManager.Money();}
}