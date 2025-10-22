using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HenrysTechUtils;

public class TechTreeUI : MonoBehaviour
{
    // need to do all da math for making the forks correctly space out by themselves and nodes being placed correctly
    // long one this, should be fun tho

    [SerializeField] TechTreeManager Manager;
    [SerializeField] GameObject NodePrefab;

    TechNode[] root_nodes;

    void Start(){
        Establish();
    }

    public void Establish(){
        Manager.Setup();
        GenerateAllTrees();
    }

    void GenerateAllTrees(){
        root_nodes = Manager.GetRootNodes();
        foreach(TechNode root in root_nodes){
            GenerateTree(root);
        }
    }

    void GenerateTree(TechNode root){
        List<int> layer_widths = root.layer_widths;
        
        // iterate thru children here
    }
}
