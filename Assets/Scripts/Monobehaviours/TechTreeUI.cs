using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HenrysTechUtils;
using static HenrysUtils;
using TMPro;

public class TechTreeUI : MonoBehaviour{

    [Header("Main")]
    [SerializeField] TechTreeManager Manager;
    [SerializeField] SoundEffectLookup SFX_Lookup;

    [Header("UI")]
    [SerializeField] TMP_Text TitleText;

    [Header("Instancing")]
    [SerializeField] GameObject NodePrefab;
    [SerializeField] RectTransform TreeHolder;
    [SerializeField] GameObject GenericHolder;

    // Tabs
    int current_tab = 0;
    List<TT_UI_Node> ui_nodes = new List<TT_UI_Node>();
    List<GameObject> tree_holders = new List<GameObject>();

    // Positioning
    Vector2 tree_position;
    const float sprint_mult = 1.5f;
    const float speed = 500f;
    const float max_width = 2000f;
    const float max_height = 1000f;

    // Zoom
    float tree_scale = 1;
    const float zoom_sensitivity = 30f;
    const float max_scale = 2.25f;
    const float min_scale = 0.5f;

    // Mouse Drag
    Vector2 start_mouse_pos;
    Vector2 mouse_position;
    bool mousing;

    // SETUP //

    void Start(){Setup();}

    public void Setup(){
        Clean();
        Establish();
        RefreshTabs();
    }

    void Clean(){
        current_tab = 0;
        for(int i = 0; i < tree_holders.Count; i++)
            GameObject.Destroy(tree_holders[i]);
        tree_holders = new List<GameObject>();
    }

    // UI INTERACTIONS //

    public void Open(){
        PlaySFX("UI_Raise", SFX_Lookup);
    }

    public void Close(){
        PlaySFX("UI_1", SFX_Lookup);
    }

    public void ChangeTab(int move){
        current_tab += move;
        if(current_tab < 0)
            current_tab = tree_holders.Count - 1;
        if(current_tab >= tree_holders.Count)
            current_tab = 0;

        PlaySFX("UI_2", SFX_Lookup);
        RefreshTabs();
    }

    // NAVIGATION //

    void Update(){ // Note, there is no 'camera' here, we are just moving the tree holder.
        MouseControls();
        KeyboardControls();
        CameraZoom();
        ApplyPosition();
    }

    void MouseControls(){
        mouse_position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        if(Input.GetMouseButtonDown(0))
            mousing = true;
        if(Input.GetMouseButtonUp(0)){
            mousing = false;
            tree_position += mouse_position - start_mouse_pos;
        }
        if(!mousing)
            start_mouse_pos = mouse_position;
    }

    void KeyboardControls(){
        if(mousing)
            return;

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
        TreeHolder.anchoredPosition = tree_position + mouse_position - start_mouse_pos;
    }

    // GENERATION //

    public void Establish(){
        tree_position = TreeHolder.anchoredPosition;
        Manager.Setup();
        GenerateAllTrees();
    }

    void GenerateAllTrees(){

        ui_nodes = new List<TT_UI_Node>();

        for(int i = 0; i < Manager.RootNodes().Length; i++){
            
            GameObject tree_master = GameObject.Instantiate(GenericHolder);
            tree_master.transform.parent = TreeHolder;
            tree_master.transform.localPosition = Vector3.zero;
            tree_master.transform.localScale = new Vector3(1f, 1f, 1f);
            tree_holders.Add(tree_master);
            tree_master.transform.name = Manager.RootObjects()[i].Title();

            GenerateNode(Manager.RootNodes()[i], tree_master.transform, 0, 0, 0);
        }

        GameObject.Destroy(GenericHolder);
    }

    void GenerateNode(TechNode node, Transform holder, int depth, int index, float parent_x){
        
        GameObject prefab = GameObject.Instantiate(NodePrefab);

        prefab.transform.parent = holder;
        prefab.transform.localPosition = Vector3.zero;
        prefab.transform.localScale = new Vector3(1, 1, 1);

        TT_UI_Node ui_node = prefab.GetComponent<TT_UI_Node>();
        ui_node.Setup(node, this, depth, node.Width(), index, parent_x);
        ui_nodes.Add(ui_node);

        if(node.HasChildren()){
            int children = node.Next().Length;
            for(int i = 0; i < children; i++){
                GenerateNode(node.Next()[i], holder, depth + 1, i, ui_node.x_position);
            }
        }

    }

    // REFRESHING //

    void RefreshTabs(){
        for(int i = 0; i < tree_holders.Count; i++)
            tree_holders[i].SetActive(i == current_tab);
        TitleText.text = Manager.RootObjects()[current_tab].Title();
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
