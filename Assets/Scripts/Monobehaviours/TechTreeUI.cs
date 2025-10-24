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
    [SerializeField] Transform Holder;

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
            GenerateNode(root, 0, 0, 0);
        }
    }

    void GenerateNode(TechNode node, int depth, int index, float parent_x){
        
        GameObject prefab = GameObject.Instantiate(NodePrefab);

        prefab.transform.parent = Holder;
        prefab.transform.localPosition = Vector3.zero;
        prefab.transform.localScale = new Vector3(1, 1, 1);

        TT_UI_Node ui_node = prefab.GetComponent<TT_UI_Node>();
        ui_node.Setup(node, depth, node.Width(), index, parent_x);

        if(node.HasChildren()){
            int children = node.Next().Length;
            for(int i = 0; i < children; i++){
                GenerateNode(node.Next()[i], depth + 1, i, ui_node.x_position);
            }
        }

    }
}
