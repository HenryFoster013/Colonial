using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HenrysMapUtils;
using static HenrysUtils;

public class PreviewRenderer : MonoBehaviour{
    [SerializeField] GameObject RendererPrefab;
    [SerializeField] GameObject ButtonPrefab;
    PieceData piece_data;
    int reference;
    bool troop_building;
    Camera cam;
    int item_cost;
    PlayerManager player;
    SpawnButton button_manager;
    GameObject button;
    TMP_Text cost_text;
    Transform renderer_transform;
    GameObject tile_model;

    public void Setup(Transform renderer_holder, Transform button_holder, int iteration, RenderTextureDescriptor rend_text_desc, PlayerManager _player, Color colour, int cost, bool _troop_building, PieceData data){
        reference = iteration;
        item_cost = cost;
        player = _player;
        troop_building = _troop_building;
        piece_data = data;
        
        GameObject g = GameObject.Instantiate(RendererPrefab);
        renderer_transform = g.transform;
        RenderTexture rend_text = new RenderTexture(rend_text_desc);
        rend_text.Create();
        renderer_transform.parent = renderer_holder;
        renderer_transform.position = new Vector3(12 * (reference + 1), 0, 0);
        cam = renderer_transform.GetChild(1).GetChild(0).GetComponent<Camera>();
        cam.targetTexture = rend_text;

        button = GameObject.Instantiate(ButtonPrefab, button_holder.position, Quaternion.identity);
        button.transform.parent = button_holder;
        button.transform.localScale = new Vector3(1,1,1);
        button_manager = button.transform.GetComponent<SpawnButton>();

        cost_text = button.transform.GetChild(0).GetChild(3).GetComponent<TMP_Text>();

        if(!troop_building)
            player.SpawnModelHolderTroop(reference, g.transform.GetChild(0));
        else
            player.SpawnModelHolderBuildng(reference, g.transform.GetChild(0));

        button_manager.Setup(this, colour, cost, rend_text);
    }

    public PieceData GetPieceData(){
        return piece_data;
    }

    public void SetTile(Tile tile){
        if(!troop_building)
            return;

        GameObject.Destroy(tile_model);
        tile_model = GameObject.Instantiate(tile.type.Prefab(), renderer_transform.position, renderer_transform.rotation);
        tile_model.transform.parent = renderer_transform;
        SetLayer(tile_model, 7);
        renderer_transform.transform.position = new Vector3(renderer_transform.transform.position.x, tile.world_position.y, renderer_transform.transform.position.z);
    }

    public void UpdateMoney(int money){
        if(money >= item_cost)
            cost_text.color = Color.white;
        else
            cost_text.color = Color.red;
    }

    public void SetPosition(Vector2 pos){
        button.GetComponent<RectTransform>().anchoredPosition = pos;
    }

    public void Disable(){
        cam.gameObject.SetActive(false);
        renderer_transform.gameObject.SetActive(false);
        button.SetActive(false);
    }

    public void Enable(){
        cam.gameObject.SetActive(true);
        renderer_transform.gameObject.SetActive(true);
        button.SetActive(true);
    }
    
    public void PressButton(){
        if(troop_building)
            player.SpawnBuildingButton(piece_data);
        else
            player.SpawnTroopButton(reference);
    }
}