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
    [SerializeField] RectTransform TreeHolder;
    [SerializeField] Transform NodeHolder;

    TechNode[] root_nodes;
    List<TT_UI_Node> ui_nodes = new List<TT_UI_Node>();

    Vector2 tree_position = Vector2.zero;
   const float sprint_mult = 1.5f;
   const float speed = 200f;
   const float max_width = 1000f;
   const float max_height = 1000f;

    float tree_scale = 1;
    const float zoom_sensitivity = 30f;
    const float max_scale = 2.25f;
    const float min_scale = 0.5f;

    void Start(){
        Establish();
    }

    void Update(){
        Navigation();
    }

    // NAVIGATION //

    void Navigation(){
        
        // Note, there is no 'camera' here, we are just moving the tree holder.

        KeyboardControls();
        CameraZoom();
        ApplyPosition();
    }

    void KeyboardControls(){

        float sprint = 1;
        if(Input.GetKey(KeyCode.LeftShift))
            sprint = sprint_mult;
        
        Vector2 dir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        tree_position += dir.normalized * speed * Time.deltaTime * sprint * -1;
    }

    void CameraZoom(){
        tree_scale += Input.GetAxis("Mouse ScrollWheel") * zoom_sensitivity * Time.deltaTime;
        tree_scale = Mathf.Clamp(tree_scale, min_scale, max_scale);
        TreeHolder.localScale = new Vector3(tree_scale, tree_scale, tree_scale);
    }

    void ApplyPosition(){
        tree_position = new Vector2(Mathf.Clamp(tree_position.x, -max_width, max_width), Mathf.Clamp(tree_position.y, -max_height, max_height));
        TreeHolder.anchoredPosition = tree_position;
    }

    // GENERATION //

    public void Establish(){
        Manager.Setup();
        GenerateAllTrees();
    }

    void GenerateAllTrees(){
        ui_nodes = new List<TT_UI_Node>();
        root_nodes = Manager.GetRootNodes();
        foreach(TechNode root in root_nodes){
            GenerateNode(root, 0, 0, 0);
        }
    }

    void GenerateNode(TechNode node, int depth, int index, float parent_x){
        
        GameObject prefab = GameObject.Instantiate(NodePrefab);

        prefab.transform.parent = NodeHolder;
        prefab.transform.localPosition = Vector3.zero;
        prefab.transform.localScale = new Vector3(1, 1, 1);

        TT_UI_Node ui_node = prefab.GetComponent<TT_UI_Node>();
        ui_node.Setup(node, this, depth, node.Width(), index, parent_x);
        ui_nodes.Add(ui_node);

        if(node.HasChildren()){
            int children = node.Next().Length;
            for(int i = 0; i < children; i++){
                GenerateNode(node.Next()[i], depth + 1, i, ui_node.x_position);
            }
        }

    }

    public void Refresh(){
        RefreshAllColours();
    }

    void RefreshAllColours(){
        foreach(TT_UI_Node ui_node in ui_nodes){
            ui_node.RefreshColours();
        }
    }
}
