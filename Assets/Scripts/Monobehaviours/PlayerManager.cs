using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;
using static HenrysUtils;
using HenrysMapUtils;

public class PlayerManager : MonoBehaviour
{
    [Header("--- MAIN ---")]
    [SerializeField] MapManager Map;
    [SerializeField] TileLookup _TileLookup;
    [SerializeField] PieceLookup _PieceLookup;
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] TroopLookup _TroopLookup;
    [SerializeField] SoundEffectLookup SFX_Lookup;
    [SerializeField] SessionManager _SessionManager;
    [SerializeField] GameplayManager _GameplayManager;
    
    [Header(" --- CAMERA --- ")]
    [SerializeField] Camera _Camera;
    [SerializeField] Transform CameraHook;
    [SerializeField] Transform CameraSpine;

    [Header(" --- TROOPS --- ")]
    [HideInInspector] public bool OurTurn = false;
    bool[] troops_owned;

    [Header(" --- UI --- ")]
    [SerializeField] GameObject TroopSpawnMenu;
    [SerializeField] GameObject EndTurnButton;
    [SerializeField] Animator TurnNameAnim;
    [SerializeField] TMP_Text TurnNameText;

    [Header("Tile Popup")]
    [SerializeField] GameObject TileInfoDisplay;
    [SerializeField] Transform TileRenderCamera;
    [SerializeField] Transform TileRenderPoint;
    [SerializeField] Transform TileModelHolder;
    [SerializeField] Image DisplayBG;

    [Header("Troop Popups")]
    [SerializeField] GameObject TroopButton;
    [SerializeField] Transform TroopButtonHolder;
    [SerializeField] GameObject TroopRenderer;
    [SerializeField] Transform TroopRendererHolder;
    RenderTexture[] troop_renders;
    Transform[] troop_buttons;

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

    public List<Troop> OurTroops = new List<Troop>();
    List<Transform> blue_grid_highlights = new List<Transform>();
    List<Transform> red_grid_highlights = new List<Transform>();
    List<Tile> walkable_tiles = new List<Tile>();
    List<Tile> special_tiles = new List<Tile>();
    public List<Tile> attackable_tiles = new List<Tile>();
    Tile current_tile;
    bool block_world_clicks = false;

    Troop current_troop;

    const int HiddenLayer = 7;

    // BASE //

    public void Setup(){
        GeneralSetup();
        Deselect();
    }

    void Update(){
        CameraLogic();
        ClickingLogic();
        SelectionLogic();
        Animate();
    }

    // SETUP //

    void GeneralSetup(){
        GenerateTroopRenderers();
        ResetCameraRot();
        ResetUI();
    }

    void ResetCameraRot(){
        CameraSpine.rotation = Quaternion.AngleAxis(target_spine_rot, Vector3.up);
        CameraHook.localEulerAngles = new Vector3(GetCameraXRot(), 0, 0);
    }

    void ResetUI(){
        TroopSpawnMenu.SetActive(false);
    }

    // ANIMATION //

    void Animate(){
        WateryHighlights();
        EndTurnButton.SetActive(OurTurn);
        TurnDisplay.text = _GameplayManager.current_turn.ToString();
        StarsDisplay.text = _GameplayManager.current_stars.ToString();
    }

    void WateryHighlights(){
        if(!Map.AnimatedWater || !Map.ready)
            return;

        if(current_tile != null){
            BaseHighlight.transform.position = current_tile.world_position;
        }
        for(int i = 0; i < walkable_tiles.Count; i++){
            if(blue_grid_highlights.Count >= walkable_tiles.Count){
                if(blue_grid_highlights[i] != null){
                    blue_grid_highlights[i].position = walkable_tiles[i].world_position;
                }
            }
            if(red_grid_highlights.Count >= walkable_tiles.Count){
                if(red_grid_highlights[i] != null){
                    red_grid_highlights[i].position = walkable_tiles[i].world_position;
                }
            }
        }
    }

    // CLICKING //

    Vector2Int GridConversion(Vector2Int pos){
        return new Vector2Int(Map.MapSize - pos.y - 1,pos.x);
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
        TroopData[] troops = _SessionManager.LocalFactionData().Troops();

        if(troops[i].Cost() <= _GameplayManager.current_stars){
            _GameplayManager.SpendStars(troops[i].Cost());

            block_world_clicks = true;
            TroopSpawnMenu.SetActive(false);
            if(i > -1 && i < troops.Length){
                _GameplayManager.RPC_SpawnTroop(_TroopLookup.ID(troops[i]), current_tile.ID, _SessionManager.OurInstance.ID);
            }
            Deselect();
        }
        else
            PlaySFX("UI_Error_2", SFX_Lookup);
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
                   SelectTile(Map.GetTileFromTransform(hit.transform.parent));
                }
                else if(hit.transform.tag == "Troop"){
                    SelectTroop(hit.transform.gameObject.GetComponent<Troop>());
                }
            }
            else{
                Deselect();
                PlaySFX("Tap High", SFX_Lookup);
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

    void SetupTiles(Troop troop){
        // Priority = attacking, special, walkable
        walkable_tiles = new List<Tile>();
        special_tiles = new List<Tile>();
        attackable_tiles = new List<Tile>();

        if(!troop.UsedSpecial()){
            attackable_tiles = _GameplayManager.EnemyTileFilter(Map.TilesByDistance(troop.current_tile, troop.Data.AttackDistance(), false));
            special_tiles = _GameplayManager.SpecialTileFilter(Map.TilesByDistance(troop.current_tile, 1, false));
            special_tiles = special_tiles.Except(attackable_tiles).ToList();
        }

        if(!troop.UsedMove()){
            walkable_tiles = _GameplayManager.WalkableTileFilter(Map.TilesByDistance(troop.current_tile, troop.Data.MoveDistance(), true));
            walkable_tiles = walkable_tiles.Except(attackable_tiles).ToList();
            walkable_tiles = walkable_tiles.Except(special_tiles).ToList();
        }
    }

    public bool CheckNoSpecials(Troop troop){
        special_tiles = new List<Tile>();
        attackable_tiles = new List<Tile>();

        if(!troop.UsedSpecial()){
            attackable_tiles = _GameplayManager.EnemyTileFilter(Map.TilesByDistance(troop.current_tile, troop.Data.AttackDistance(), false));
            special_tiles = _GameplayManager.SpecialTileFilter(Map.TilesByDistance(troop.current_tile, 1, false));
            special_tiles = special_tiles.Except(attackable_tiles).ToList();
        }

        return (special_tiles.Count == 0 && attackable_tiles.Count == 0);
    }

    void GetTroopRanges(Troop troop){

        if(troop.Owner != _SessionManager.OurInstance.ID)
            return;

        SetupTiles(troop);

        // DISPLAY HIGHLIGHTS //
        CheckHighlightCount(blue_grid_highlights, walkable_tiles.Count, BlueHighlightPrefab, BlueHighlightHook);
        CheckHighlightCount(red_grid_highlights, attackable_tiles.Count, RedHighlightPrefab, RedHighlightHook);

        foreach(Transform t in blue_grid_highlights)
            t.gameObject.SetActive(false);
        foreach(Transform t in red_grid_highlights)
            t.gameObject.SetActive(false);

        if(!troop.UsedMove()){
            for(int i = 0; i < walkable_tiles.Count; i++){
                blue_grid_highlights[i].position = walkable_tiles[i].world_position;
                blue_grid_highlights[i].gameObject.SetActive(true);
            }
        }

        if(!troop.UsedSpecial()){
            for(int i = 0; i < attackable_tiles.Count; i++){
                red_grid_highlights[i].position = attackable_tiles[i].world_position;
                red_grid_highlights[i].gameObject.SetActive(true);
            }
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

    void SelectTile(Tile tile){
        if(current_troop != null){
            if(walkable_tiles.Contains(tile)){
                MoveTroop(tile);
            }
            else if(attackable_tiles.Contains(tile)){
                AttackTroop(_GameplayManager.GetTroopAt(tile));
            }
            else
                StandardTileSelect(tile);
        }
        else
            StandardTileSelect(tile);
    }

    void MoveTroop(Tile tile){
        _GameplayManager.RPC_SetTroopPos(current_troop.UniqueID, tile.ID, _SessionManager.OurInstance.ID);
        current_troop.UseMove();
        Deselect();
    }

    void StandardTileSelect(Tile tile){
        Deselect();

        current_tile = tile;

        DisplayBG.color = current_tile.type.TabColour();
        BaseHighlightMaterial.SetColor("_BaseColor", new Color(1f,1f,1f,1f));

        foreach(Transform trans in TileModelHolder)
            Destroy(trans.gameObject);

        GameObject t = GameObject.Instantiate(current_tile.type.Prefab(), TileModelHolder.position, TileModelHolder.rotation);
        t.transform.parent = TileModelHolder;
        SetLayer(t, HiddenLayer);
        
        GameObject p = GameObject.Instantiate(current_tile.piece.Prefab(), TileModelHolder.position, TileModelHolder.rotation);
        p.transform.parent = TileModelHolder;
        SetLayer(p, HiddenLayer);

        if(current_tile.piece.ContainsBillboards()){
            foreach(Transform _child in p.transform){
                if(_child.tag == "Billboard"){
                    Billboard b = _child.GetComponent<Billboard>();
                    if(b != null)
                        b.m_Camera = TileRenderCamera; 
                }
            }
        }
        
        BaseHighlight.transform.position = current_tile.world_position;
        BaseHighlight.SetActive(true);

        TileInfoDisplay.SetActive(true);
        TileRenderPoint.position = new Vector3(TileRenderPoint.position.x, current_tile.world_position.y, TileRenderPoint.position.z);

        CheckTileData(tile);

        PlaySFX("Tap", SFX_Lookup);
    }

    void CheckTileData(Tile tile){
        TroopSpawnMenu.SetActive(false);
        if(tile.piece.CanSpawnTroops()){
            if(!_GameplayManager.TroopOnTile(tile) && OurTurn){
                if(Map.CheckTileOwnership(tile, _SessionManager.LocalFactionData()))
                    OpenSpawnMenu();
            }
        }
    }

    bool IsTroopAttackable(Troop troop){
        bool result = false;
        if(current_troop != null){
            bool current_is_ours = current_troop.FactionID() == _SessionManager.OurInstance.Faction_ID;
            bool target_not_ours = troop.FactionID() != _SessionManager.OurInstance.Faction_ID;
            bool in_range = attackable_tiles.Contains(Map.GetTile(troop.current_tile));

            result = current_is_ours && target_not_ours && in_range;
        }

        return result;
    }

    void AttackTroop(Troop troop){
        if(troop != null && current_troop != null){
            _GameplayManager.RPC_AttackTroop(current_troop.UniqueID, troop.UniqueID, true);
        }
        Deselect();
    }

    void SelectTroop(Troop troop){

        if(IsTroopAttackable(troop)){
            AttackTroop(troop);
            return;
        }

        Deselect();

        current_tile = Map.GetTile(troop.current_tile);
        current_troop = troop;
        troop.SetSelected(true);

        BaseHighlight.transform.position = current_tile.world_position;
        BaseHighlight.SetActive(true);

        if(!troop.TurnOver() || troop.Owner != _SessionManager.OurInstance.ID)
            BaseHighlightMaterial.SetColor("_BaseColor", troop.FactionData().Colour());
        else
            BaseHighlightMaterial.SetColor("_BaseColor", DisabledColour);

        TileRenderPoint.position = Vector3.right * 200f;

        SpawnModelHolderTroop(troop.Data, TileModelHolder, troop.FactionID());

        TileInfoDisplay.SetActive(true);
        DisplayBG.color = _SessionManager.PlayerFaction(troop.Owner).Colour();
        
        if(troop.Owner == _SessionManager.OurInstance.ID){
            print("Getting ranges");
            GetTroopRanges(troop);
        }

        PlaySFX("Pickup", SFX_Lookup);
    }

    public void AddTroop(Troop t){
        if(t.Owner == _SessionManager.OurInstance.ID){
            OurTroops.Add(t);
            t.SetEventCamera(_Camera);
        }
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

    public void Deselect(){
        TroopSpawnMenu.SetActive(false);
        if(current_troop != null)
            current_troop.SetSelected(false);
        current_troop = null;
        current_tile = null;
        walkable_tiles = new List<Tile>();
        attackable_tiles = new List<Tile>();
        ResetSelectionUI();
    }

    public void DisableAllTroops(){
        foreach(Troop t in OurTroops){
            if(t != null)
                t.EndTurn();
        }
    }

    public void EnableAllTroops(){
        foreach(Troop t in OurTroops){
            if(t != null){
                t.NewTurn();
                t.EnableConquest(Map.ForeignFortress(t.current_tile));
            }
        }
    }

    // UI //

    public void UpdateTurnNameDisplay(string name){
        TurnNameText.text = name;
        TurnNameAnim.Play("Fadeout", -1, 0);
    }

    void ResetSelectionUI(){
        TileInfoDisplay.SetActive(false);
        foreach(Transform t in blue_grid_highlights)
            t.gameObject.SetActive(false);
        foreach(Transform t in red_grid_highlights)
            t.gameObject.SetActive(false);
        BaseHighlight.SetActive(false);
    }

    public void EndTurn(){
        Deselect();
        _GameplayManager.RPC_AskToMoveTurn(_SessionManager.OurInstance.ID);
        PlaySFX("Drums_2", SFX_Lookup);
    }

    void GenerateTroopRenderers(){
        TroopData[] troops = _SessionManager.LocalFactionData().Troops();
        troop_renders = new RenderTexture[troops.Length];
        troop_buttons = new Transform[troops.Length];
        
        // REPLACE THIS IN FUTURE
        // Acts as a mask for which troops we have access to (for the tech tree)
        troops_owned = new bool[troops.Length];
        for(int i = 0; i < troops_owned.Length; i++){
            troops_owned[i] = true;
        }

        for(int count = 0; count < troops.Length; count++){

            GameObject g = GameObject.Instantiate(TroopRenderer);
            RenderTexture rendtext = new RenderTexture(333, 512, 24);
            rendtext.Create();
            g.transform.parent = TroopRendererHolder;
            g.transform.position = new Vector3(25 * (count + 1), 0, 0);
            g.transform.GetChild(1).GetChild(0).GetComponent<Camera>().targetTexture = rendtext;
            troop_renders[count] = rendtext;
            SpawnModelHolderTroop(troops[count], g.transform.GetChild(0), _SessionManager.LocalFactionID());

            GameObject b = GameObject.Instantiate(TroopButton, TroopButtonHolder.position, Quaternion.identity);
            b.transform.parent = TroopButtonHolder;
            b.transform.GetComponent<TroopButton>().Setup(count, this, _SessionManager.LocalFactionData().Colour(), troops[count].Cost(), rendtext);
            troop_buttons[count] = b.transform;
        }
    }

    void OpenSpawnMenu(){

        foreach(Transform t in troop_buttons)
            t.gameObject.SetActive(false);

        int total_troops = 0;
        for(int i = 0; i < troops_owned.Length; i++){
            if(troops_owned[i]){
                total_troops++;
            }
        }

        float offy = 120;
        float centering = (-1 * offy * total_troops) / 2;
        centering += (offy / 2);
        int active_count = 0;

        for(int i = 0; i < troops_owned.Length; i++){
            if(troops_owned[i]){
                TMP_Text cost_text = troop_buttons[i].GetChild(3).GetComponent<TMP_Text>();
                cost_text.color = Color.white;
                if(_SessionManager.LocalFactionData().Troops()[i].Cost() > _GameplayManager.current_stars)
                    cost_text.color = Color.red;
                troop_buttons[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2((active_count * offy) + centering, 0);
                troop_buttons[i].gameObject.SetActive(true);
                active_count++;
            }
        }        

        TroopSpawnMenu.SetActive(true);
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
            Mathf.Clamp(target_camera_pos.x, 0, Map.MapSize), 
            Mathf.Clamp(target_camera_pos.y, 2.5f, 17.5f),
            Mathf.Clamp(target_camera_pos.z, -8f, Map.MapSize + 8f));
    }

    void ApplyCameraChanges(){
        CameraSpine.rotation = Quaternion.Lerp(CameraSpine.rotation, Quaternion.AngleAxis(target_spine_rot, Vector3.up), Time.deltaTime * 6f);
        CameraHook.localEulerAngles = Vector3.Lerp(CameraHook.localEulerAngles, new Vector3(GetCameraXRot(), 0, 0), Time.deltaTime * 6f);
        CameraSpine.position = Vector3.Lerp(CameraSpine.position, target_camera_pos, Time.deltaTime * 6f);
    }

    float GetCameraXRot(){return 25f + ((CameraHook.position.y - 5f) * 4f);}
}
