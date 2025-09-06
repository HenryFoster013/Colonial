using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;

public class PlayerManager : MonoBehaviour
{
    [Header("--- MAIN ---")]
    [SerializeField] MapManager Map;
    [SerializeField] TileLookup _TileLookup;
    [SerializeField] PieceLookup _PieceLookup;
    [SerializeField] FactionLookup _FactionLookup;
    SessionManager _SessionManager;
    GameplayManager _GameplayManager;
    
    [Header(" --- CAMERA --- ")]
    [SerializeField] Camera _Camera;
    [SerializeField] Transform CameraHook;
    [SerializeField] Transform CameraSpine;

    [Header(" --- TROOPS --- ")]
    public List<TroopData> SpawnableTroops = new List<TroopData>();

    [Header(" --- UI --- ")]
    [SerializeField] GameObject TroopSpawnMenu;

    [Header("Tile Popup")]
    [SerializeField] GameObject TileInfoDisplay;
    [SerializeField] Transform TileRenderCamera;
    [SerializeField] Transform TileRenderPoint;
    [SerializeField] Transform TileModelHolder;
    [SerializeField] Image DisplayBG;

    [Header("Troop Popups")]
    [SerializeField] Transform[] TroopButtons;
    [SerializeField] Transform[] TroopModelHolders;

    [Header("Turns")]
    [SerializeField] TMP_Text StarsDisplay;
    [SerializeField] TMP_Text TurnDisplay;

    [Header("Highlights")]
    [SerializeField] LayerMask ClickableLayers;
    [SerializeField] GameObject BaseHighlight;
    [SerializeField] Material BaseHighlightMaterial;
    [SerializeField] GameObject BlueHighlightPrefab;
    [SerializeField] Transform BlueHighlightHook;
    [SerializeField] GameObject RedHighlightPrefab;
    [SerializeField] Transform RedHighlightHook;
    [SerializeField] Color DisabledColour;
     
    float camera_speed = 7f;
    float camera_sprnt_mult = 2f;
    float camera_vert_speed = 150f;
    Vector3 camera_start_point;
    Vector3 target_camera_pos = new Vector3(0f, 6f, 0f);
    float camera_spin;
    float target_spine_rot = -90;

    List<Transform> blue_grid_highlights = new List<Transform>();
    List<Transform> red_grid_highlights = new List<Transform>();
    List<int> walkable_tiles = new List<int>();
    List<int> special_tiles = new List<int>();
    List<int> attackable_tiles = new List<int>();
    int current_tile;
    bool block_world_clicks = false;

    Troop current_troop;

    const int HiddenLayer = 7;

    // BASE //

    public void Setup(SessionManager sm, GameplayManager gm){
        _GameplayManager = gm;
        _SessionManager = sm;
        GeneralSetup();
        Deselect();
    }

    void Update(){
        CameraLogic();
        ClickingLogic();
        SelectionLogic();
        Animate();
    }

    List<Troop> OurTroops(){
        return _GameplayManager.GetTroops(_SessionManager.OurInstance.ID);
    }

    public void EndTurn(){
        Deselect();
        _GameplayManager.UpTurn();
        _GameplayManager.UpStars();
        TurnDisplay.text = _GameplayManager.current_turn.ToString();
        StarsDisplay.text = _GameplayManager.current_stars.ToString();
        foreach(Troop t in OurTroops())
            t.NewTurn();
    }

    // SETUP //

    void GeneralSetup(){
        SetupTroops();
        ResetCameraRot();
        ResetUI();
    }

    void SetupTroops(){
        SpawnableTroops = new List<TroopData>();
        SpawnableTroops.AddRange(_SessionManager.LocalFactionData().Troops());
    }

    void ResetCameraRot(){
        CameraSpine.rotation = Quaternion.AngleAxis(target_spine_rot, Vector3.up);
        CameraHook.localEulerAngles = new Vector3(GetCameraXRot(), 0, 0);
    }

    void ResetUI(){
        TroopSpawnMenu.SetActive(false);
        StarsDisplay.text = _GameplayManager.current_stars.ToString();
        TurnDisplay.text = _GameplayManager.current_turn.ToString();
    }

    // ANIMATION //

    void Animate(){
        WateryHighlights();
    }

    void WateryHighlights(){
        if(!Map.AnimatedWater || !Map.Ready())
            return;

        if(_TileLookup.Tile(Map.GetTileType(current_tile)).CheckType("WATER")){
            BaseHighlight.transform.position = Map.GetTilePosition(current_tile);
        }
        for(int i = 0; i < walkable_tiles.Count; i++){
            if(blue_grid_highlights.Count >= walkable_tiles.Count){
                if(blue_grid_highlights[i] != null){
                    blue_grid_highlights[i].position = Map.GetTilePosition(walkable_tiles[i]);
                }
            }
            if(red_grid_highlights.Count >= walkable_tiles.Count){
                if(red_grid_highlights[i] != null){
                    red_grid_highlights[i].position = Map.GetTilePosition(walkable_tiles[i]);
                }
            }
        }
    }

    // CLICKING //

    Vector2Int GridConversion(Vector2Int pos){
        return new Vector2Int(Map.GetSize() - pos.y - 1,pos.x);
    }

    float downTime = 0;
    Vector3 mouse_pos;
    Vector3 mouse_start_pos;

    // bool holds the clock for an extra frame for comparisons.
    bool click_buffer;
    void ClickingLogic(){
        if(Input.GetMouseButtonDown(0))
            mouse_start_pos = Input.mousePosition;
        mouse_pos = Input.mousePosition;

        if(Input.GetMouseButton(0))
            downTime += Time.deltaTime;
        else{
            if(!click_buffer){
                click_buffer = true;
            }
            else{
                downTime = 0;
                click_buffer = false;
            }
        }
    }

    bool ShortClick(){
        return !IsPointerOverUIElement() && (Input.GetMouseButtonUp(0) && downTime < 0.2f && ((mouse_pos - mouse_start_pos).sqrMagnitude < 10f));
    }

    bool LongClick(){
        return !IsPointerOverUIElement() && (Input.GetMouseButton(0) && (downTime > 0.2f || ((mouse_pos - mouse_start_pos).sqrMagnitude > 10f)));
    }

    // SELECTION //

    public void SpawnTroopButton(int i){
        if(SpawnableTroops[i].Cost() <= _GameplayManager.current_stars){
            _GameplayManager.SpendStars(SpawnableTroops[i].Cost());
            StarsDisplay.text = _GameplayManager.current_stars.ToString();

            block_world_clicks = true;
            TroopSpawnMenu.SetActive(false);
            if(i > -1 && i < SpawnableTroops.Count){
                _GameplayManager.AskToSpawnTroop(SpawnableTroops[i], current_tile, _SessionManager.OurInstance.ID);
            }
            Deselect();
        }
    }

    void SelectionLogic(){
        if(block_world_clicks){
            block_world_clicks = false;
            return;
        }

        if (ShortClick()){ 
            RaycastHit hit;
            Ray ray = _Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100f, ClickableLayers)){
                if(hit.transform.tag == "Tile"){
                   SelectTile(Map.GetTileIDFromTransform(hit.transform.parent));
                }
                else if(hit.transform.tag == "Troop"){
                    SelectTroop(hit.transform.gameObject.GetComponent<Troop>());
                }
            }
            else{
                Deselect();
            }
        }

        if(Input.GetMouseButtonDown(0)){
            camera_start_point = CameraHook.position;
        }
        
        if(LongClick()){
            Vector3 rationalised_mouse_offset = (CameraSpine.forward * (mouse_pos.y - mouse_start_pos.y)) + (CameraSpine.right * (mouse_pos.x - mouse_start_pos.x));
            target_camera_pos = camera_start_point + (rationalised_mouse_offset * -1 * camera_start_point.y / (Screen.height));
        }
    }

    List<int> WalkableTileFilter(List<int> tiles){
        foreach(Troop t in OurTroops())
            tiles.Remove(t.GetTile());
        return tiles;
    }

    void GetTroopRanges(Troop troop){

        if(troop.Owner != _SessionManager.OurInstance.ID)
            return;

        // In order of priority, with attacking overwriting all
        walkable_tiles = new List<int>();
        special_tiles = new List<int>();
        attackable_tiles = new List<int>();

        if(!troop.UsedSpecial()){
            // Get attack tiles and filter for enemy troops here
            // Get special tiles and filter for special objects here
            special_tiles = special_tiles.Except(attackable_tiles).ToList();
        }

        if(!troop.UsedMove()){
            walkable_tiles = WalkableTileFilter(Map.TilesByDistance(troop.GetTile(), troop.Data.MoveDistance(), true));
            walkable_tiles = walkable_tiles.Except(attackable_tiles).ToList();
            walkable_tiles = walkable_tiles.Except(special_tiles).ToList();
        }

        // DISPLAY HIGHLIGHTS //
        CheckHighlightCount(blue_grid_highlights, walkable_tiles.Count, BlueHighlightPrefab, BlueHighlightHook);
        CheckHighlightCount(red_grid_highlights, attackable_tiles.Count, RedHighlightPrefab, RedHighlightHook);

        foreach(Transform t in blue_grid_highlights)
            t.gameObject.SetActive(false);

        for(int i = 0; i < walkable_tiles.Count; i++){
            blue_grid_highlights[i].position = Map.GetTilePosition(walkable_tiles[i]);
            blue_grid_highlights[i].gameObject.SetActive(true);
        }

        for(int i = 0; i < attackable_tiles.Count; i++){
            red_grid_highlights[i].position = Map.GetTilePosition(attackable_tiles[i]);
            red_grid_highlights[i].gameObject.SetActive(true);
        }
    }

    void CheckHighlightCount(List<Transform> highs, int to_match, GameObject prefab, Transform hook){
        int to_add = to_match - highs.Count;

        if(to_add > 0){
            for(int i = 0; i < to_add; i++){
                GameObject g = GameObject.Instantiate(prefab);
                g.SetActive(false);
                highs.Add(g.transform);
                g.transform.parent = hook;
            }
        }
    }

    void SelectTile(int id){
        if(current_troop != null){
            if(NumInList(id, walkable_tiles)){
                _GameplayManager.MoveTroop(current_troop, id);
                Deselect();
            }
            else if(NumInList(id, attackable_tiles)){
                // attack opponent here
                Deselect();
            }
            else
                StandardTileSelect(id);
        }
        else
            StandardTileSelect(id);
    }

    bool NumInList(int id, List<int> possibilites){
        bool found = false;
        for (int i = 0; i < possibilites.Count && !found; i++){
            if(possibilites[i] == id){
                found = true;
            }
        }
        return found;
    }

    void StandardTileSelect(int id){
        Deselect();

        current_tile = id;
        TileData tile = _TileLookup.Tile(Map.GetTileType(current_tile));
        PieceData piece = _PieceLookup.Piece(Map.GetPieceType(current_tile));

        DisplayBG.color = tile.TabColour();
        BaseHighlightMaterial.SetColor("_BaseColor", new Color(1f,1f,1f,1f));

        foreach(Transform trans in TileModelHolder)
            Destroy(trans.gameObject);

        GameObject t = GameObject.Instantiate(tile.Prefab(), TileModelHolder.position, TileModelHolder.rotation);
        t.transform.parent = TileModelHolder;
        SetLayer(t, HiddenLayer);
        
        GameObject p = GameObject.Instantiate(piece.Prefab(), TileModelHolder.position, TileModelHolder.rotation);
        p.transform.parent = TileModelHolder;
        SetLayer(p, HiddenLayer);

        if(piece.ContainsBillboards()){
            foreach(Transform child in p.transform){
                Billboard b = child.GetComponent<Billboard>();
                if(b != null)
                    b.m_Camera = TileRenderCamera; 
            }
        }
        
        BaseHighlight.transform.position = Map.GetTilePosition(current_tile);
        BaseHighlight.SetActive(true);

        TileInfoDisplay.SetActive(true);
        TileRenderPoint.position = new Vector3(TileRenderPoint.position.x, Map.GetTilePosition(current_tile).y, TileRenderPoint.position.z);

        CheckTileData(id, tile, piece);
    }

    void CheckTileData(int id, TileData tile_data, PieceData piece_data){
        TroopSpawnMenu.SetActive(false);
        if(piece_data.CanSpawnTroops()){
            if(!DoWeHaveTroopAt(id) && Map.CheckTileOwnership(id, _SessionManager.LocalFactionID())){
                OpenSpawnMenu();
            }
        }
    }

    bool DoWeHaveTroopAt(int tile){
        bool found = false;
        for(int i = 0; i < OurTroops().Count && !found; i++){
            if(OurTroops()[i].GetTile() == tile)
                found = true;
        }
        return found;
    }

    void OpenSpawnMenu(){
        float offy = 120;
        float centering = (-1 * offy * SpawnableTroops.Count) / 2;
        centering += (offy / 2);

        foreach(Transform t in TroopButtons)
            t.gameObject.SetActive(false);

        for(int i = 0; i < SpawnableTroops.Count; i++){
            if(TroopModelHolders[i].childCount == 0){
                SpawnModelHolderTroop(SpawnableTroops[i], TroopModelHolders[i], _SessionManager.LocalFactionID());
                TroopButtons[i].GetChild(0).GetComponent<Image>().color = _SessionManager.LocalFactionData().Colour();
                TroopButtons[i].GetChild(2).GetComponent<Image>().color = _SessionManager.LocalFactionData().Colour();
                TroopButtons[i].GetChild(3).GetComponent<TMP_Text>().text = SpawnableTroops[i].Cost().ToString();
            }

            TMP_Text cost_text = TroopButtons[i].GetChild(3).GetComponent<TMP_Text>();
            cost_text.text = SpawnableTroops[i].Cost().ToString();
            cost_text.color = Color.white;
            if(SpawnableTroops[i].Cost() > _GameplayManager.current_stars)
                cost_text.color = Color.red;

            TroopButtons[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2((i * offy) + centering, 0);
            TroopButtons[i].gameObject.SetActive(true);
        }        

        TroopSpawnMenu.SetActive(true);
    }

    void SelectTroop(Troop troop){
        Deselect();
        current_troop = troop;


        BaseHighlight.SetActive(true);
        BaseHighlight.transform.position = Map.GetTilePosition(troop.GetTile());

        if(!troop.TurnOver() || troop.Owner != _SessionManager.OurInstance.ID)
            BaseHighlightMaterial.SetColor("_BaseColor", _SessionManager.LocalFactionData().Colour());
        else
            BaseHighlightMaterial.SetColor("_BaseColor", DisabledColour);

        TileRenderPoint.position = Vector3.right * 200f;

        SpawnModelHolderTroop(troop.Data, TileModelHolder, troop.FactionID());

        TileInfoDisplay.SetActive(true);
        DisplayBG.color = _SessionManager.PlayerFaction(troop.Owner).Colour();
        
        if(troop.Owner == _SessionManager.OurInstance.ID)
            GetTroopRanges(troop);
    }

    void SpawnModelHolderTroop(TroopData troop, Transform holder, int fact_owner){
         foreach(Transform trans in holder)
            Destroy(trans.gameObject);
        GameObject t = GameObject.Instantiate(troop.Prefab(), holder.position - (Vector3.up * 1f), Quaternion.identity);
        t.transform.localScale = new Vector3(1,1,1) * 1.4f;
        t.transform.parent = holder;
        t.GetComponent<DisplayTroop>().DisplayInitialSetup(_SessionManager, fact_owner);
        SetLayer(t, HiddenLayer);
    }

    void SetLayer(GameObject obj, int _layer){
        obj.layer = _layer;
        foreach (Transform child in obj.transform){
            child.gameObject.layer = _layer;
            Transform _HasChildren = child.GetComponentInChildren<Transform>();
            if (_HasChildren != null)
                SetLayer(child.gameObject, _layer);
        }
    }

    void Deselect(){
        TroopSpawnMenu.SetActive(false);
        current_troop = null;
        current_tile = 0;
        walkable_tiles = new List<int>();
        attackable_tiles = new List<int>();
        ResetSelectionUI();
    }

    void ResetSelectionUI(){
        TileInfoDisplay.SetActive(false);
        foreach(Transform t in blue_grid_highlights)
            t.gameObject.SetActive(false);
        foreach(Transform t in red_grid_highlights)
            t.gameObject.SetActive(false);
        BaseHighlight.SetActive(false);
    }

    // MOUSE OVER UI CHECKER //

    public bool IsPointerOverUIElement(){
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }

    int UILayer = 5;
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults){
        for (int index = 0; index < eventSystemRaysastResults.Count; index++){
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }

    static List<RaycastResult> GetEventSystemRaycastResults(){
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    // CAMERA //

    void CameraLogic(){
        KeyboardCameraControls();
        CameraZoom();
        ClampCamera();
        ApplyCameraChanges();
    }

    public void SnapCameraToPosition(Vector3 pos){
        target_camera_pos = new Vector3(pos.x, target_camera_pos.y, pos.z);
        ClampCamera();
        CameraSpine.position = target_camera_pos;
    }

    void KeyboardCameraControls(){
        Vector3 camera_pos = target_camera_pos;

        // Sprint multiplier
        float sprint = 1;
        if(Input.GetKey(KeyCode.LeftShift))
            sprint = camera_sprnt_mult;
        
        // Set Velocity
        Vector3 dir = (CameraSpine.forward * Input.GetAxisRaw("Vertical")) + (CameraSpine.right * Input.GetAxisRaw("Horizontal"));
        camera_pos += Vector3.Normalize(dir) * camera_speed * Time.deltaTime * sprint;

        target_camera_pos = camera_pos;

        if(Input.GetKeyDown("e"))
            target_spine_rot += 45;
        if(Input.GetKeyDown("q"))
            target_spine_rot += -45;
    }

    void CameraZoom(){
        target_camera_pos += new Vector3(0, Input.GetAxis("Mouse ScrollWheel") * camera_vert_speed * Time.deltaTime, 0);
    }

    void ClampCamera(){
        target_camera_pos = new Vector3(
            Mathf.Clamp(target_camera_pos.x, 0, Map.GetSize()), 
            Mathf.Clamp(target_camera_pos.y, 2.5f, 17.5f),
            Mathf.Clamp(target_camera_pos.z, -8f, Map.GetSize() + 8f));
    }

    void ApplyCameraChanges(){
        CameraSpine.rotation = Quaternion.Lerp(CameraSpine.rotation, Quaternion.AngleAxis(target_spine_rot, Vector3.up), Time.deltaTime * 6f);
        CameraHook.localEulerAngles = Vector3.Lerp(CameraHook.localEulerAngles, new Vector3(GetCameraXRot(), 0, 0), Time.deltaTime * 6f);
        CameraSpine.position = Vector3.Lerp(CameraSpine.position, target_camera_pos, Time.deltaTime * 6f);
    }

    float GetCameraXRot(){return 25f + ((CameraHook.position.y - 5f) * 4f);}
}
