using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;
using static GenericUtils;
using MapUtils;
using Fusion;

public class PlayerManager : MonoBehaviour
{
    [Header("--- MAIN ---")]
    [SerializeField] MapManager Map;
    [SerializeField] TileLookup _TileLookup;
    [SerializeField] PieceLookup _PieceLookup;
    [SerializeField] TroopLookup _TroopLookup;
    [SerializeField] SoundEffectLookup SFX_Lookup;
    [SerializeField] SessionManager _SessionManager;
    [SerializeField] GameplayManager _GameplayManager;
    [SerializeField] TechTreeManager _TechTreeManager;
    
    [Header(" --- CAMERA --- ")]
    [SerializeField] Camera _Camera;
    [SerializeField] Transform CameraHook;
    [SerializeField] Transform CameraSpine;

    [Header(" --- TROOPS --- ")]
    [HideInInspector] public bool OurTurn = false;

    [Header(" --- UI --- ")]
    [SerializeField] GameObject SpawnMenu;
    [SerializeField] GameObject SpawnMenuArrowsHolder;
    [SerializeField] GameObject EndTurnButton;
    [SerializeField] TMP_Text CoinDisplay;
    [SerializeField] Leaderboard LeaderboardWindow;

    [Header("Turns")]
    [SerializeField] TMP_Text TurnDisplay;
    [SerializeField] Animator TurnNameAnim;
    [SerializeField] TMP_Text TurnNameText;

    [Header("Faction")]
    [SerializeField] Image FactionFlag;
    [SerializeField] TMP_Text FactionName;

    [Header("Tile Popup")]
    [SerializeField] GameObject TileInfoDisplay;
    [SerializeField] Transform TileRenderCamera;
    [SerializeField] Transform TileRenderPoint;
    [SerializeField] Transform TileModelHolder;
    [SerializeField] Image DisplayBG;

    [Header("Troop Info")]
    [SerializeField] GameObject TroopInfoDisplay;
    [SerializeField] TMP_Text TroopName;
    [SerializeField] Image TroopName_BG;
    [SerializeField] SpawnInfoDisplay SpawnInfoPopup;

    [Header("Forts")]
    [SerializeField] GameObject FortStats;
    [SerializeField] TMP_Text FortName;
    [SerializeField] Image FortName_BG;
    [SerializeField] StatBar[] StatBars;
    [SerializeField] FortressUpgradeWindow UpgradeWindow;
    [SerializeField] GameObject UpgradeButton;

    [Header("General Popups")]
    [SerializeField] Transform PreviewRendererHolder;
    [SerializeField] GameObject PreviewRendererPrefab;

    [Header("Troop Popups")]
    [SerializeField] Transform TroopButtonHolder;
    [SerializeField] Transform TroopRendererHolder;

    [Header("Building Popups")]
    [SerializeField] Transform BuildingButtonHolder;
    [SerializeField] Transform BuildingRendererHolder;

    [Header("Highlights")]
    [SerializeField] LayerMask ClickableLayers;
    [SerializeField] GameObject BaseHighlight;
    [SerializeField] Material BaseHighlightMaterial;
    [SerializeField] GameObject BlueHighlightPrefab;
    [SerializeField] Transform BlueHighlightHook;
    [SerializeField] GameObject RedHighlightPrefab;
    [SerializeField] Transform RedHighlightHook;
    [SerializeField] Color DisabledColour;

    bool pause_inputs;
     
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
    bool IsPointerOverUIElementBuffer = false;

    Troop current_troop;
    const int HiddenLayer = 7;

    int spawn_menu_offset = 0;
    PreviewRenderer[] troop_renders;
    PreviewRenderer[] building_renders;
    Camera[] troop_cameras;
    Camera[] building_cameras;

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
        GenerateRenderers();
        ResetCameraRot();
        ResetUI();
    }

    void ResetCameraRot(){
        CameraSpine.rotation = Quaternion.AngleAxis(target_spine_rot, Vector3.up);
        CameraHook.localEulerAngles = new Vector3(GetCameraXRot(), 0, 0);
    }

    void ResetUI(){
        CloseSpawnMenu();
        pause_inputs = false;
        _TechTreeManager.Setup(_SessionManager.LocalFactionData());
        FactionName.text = _SessionManager.LocalFactionData().Name().Replace(' ', '\n');
        FactionFlag.sprite = _SessionManager.LocalFactionData().Flag();
        UpgradeWindow.SilentClose();
    }

    // ANIMATION //

    void Animate(){
        WateryHighlights();
        SpawnInfoPopup.transform.position = Input.mousePosition;
        EndTurnButton.SetActive(OurTurn);
        TurnDisplay.text = "Turn " + _GameplayManager.current_turn.ToString();
        CoinDisplay.text = _SessionManager.LocalFactionData().CurrencyFormat(_SessionManager.Money());
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
        return !IsPointerOverUIElementBuffer && !IsPointerOverUIElement() && (Input.GetMouseButtonUp(0) && downTime < 0.2f && ((mouse_pos - mouse_start_pos).sqrMagnitude < 10f));
    }

    bool LongClick(){
        return !IsPointerOverUIElement() && (Input.GetMouseButton(0) && (downTime > 0.2f || ((mouse_pos - mouse_start_pos).sqrMagnitude > 10f)));
    }

    // SELECTION //

    public void SpawnBuildingButton(PieceData piece){
        if(CanAfford(piece.Cost())){
            _GameplayManager.RPC_SpawnBuilding(current_tile.ID, _PieceLookup.ID(piece));
            CloseSpawnMenu();
            Deselect();
        }
        else
            PlaySFX("UI_Error_2", SFX_Lookup);
    }

    public void SpawnTroopButton(int i){
        TroopData[] troops = _SessionManager.LocalFactionData().Troops();
        if(i > -1 && i < troops.Length){

            TroopData troop = troops[i];

            if(CanSpawnTroop(i)){
                SpendMoney(troops[i].Cost());
                block_world_clicks = true;
                _GameplayManager.RPC_SpawnTroop(_TroopLookup.ID(troop), current_tile.ID, _SessionManager.OurInstance.ID);
                CloseSpawnMenu();
                Deselect();
            }
            else
                PlaySFX("UI_Error_2", SFX_Lookup);
        }
    }

    public bool CanSpawnTroop(int troop_id){
        TroopData troop = _SessionManager.LocalFactionData().Troops()[troop_id];
        bool valid = _TechTreeManager.Unlocked(troop);
        if(valid)
            valid = CanAfford(troop.Cost());
        if(valid)
            valid = _GameplayManager.ValidTroopSpawn(troop, current_tile);
        return valid;
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

        IsPointerOverUIElementBuffer = IsPointerOverUIElement();
    }

    void SetupTiles(Troop troop){
        // Priority = attacking, special, walkable
        walkable_tiles = new List<Tile>();
        special_tiles = new List<Tile>();
        attackable_tiles = new List<Tile>();

        if(!ValidateTroop(troop))
            return;

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

        if(!ValidateTroop(troop))
            return true;

        if(!troop.UsedSpecial()){
            attackable_tiles = _GameplayManager.EnemyTileFilter(Map.TilesByDistance(troop.current_tile, troop.Data.AttackDistance(), false));
            special_tiles = _GameplayManager.SpecialTileFilter(Map.TilesByDistance(troop.current_tile, 1, false));
            special_tiles = special_tiles.Except(attackable_tiles).ToList();
        }

        return (special_tiles.Count == 0 && attackable_tiles.Count == 0);
    }

    void GetTroopRanges(Troop troop){

        if(!ValidateTroop(troop))
            return;

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
        CloseSpawnMenu();
        FortStats.SetActive(false);
        if(Map.CheckTileOwnership(tile, _SessionManager.LocalFactionData())){
            if(tile.piece.Fort()){
                if(!_GameplayManager.TroopOnTile(tile) && OurTurn)
                    OpenSpawnMenu(false);
                SetupTileStats(Map.GetTile(tile.ID).stats);
            }
            else{
                if(OurTurn)
                    OpenSpawnMenu(true);
            }
        }
    }

    void SetupTileStats(TileStats stats){
        if(stats == null)
            return;
        
        FortStats.SetActive(true);
        UpgradeButton.SetActive(OurTurn && CanAfford(stats.UpgradeCost()) && stats.BelowLevelLimit());
        FortName.text = stats.name + " (" + _SessionManager.LocalFactionData().CurrencyFormat(stats.Value()) + ")";
        FortName_BG.color = stats.tile.owner.Colour();
        StatBars[0].Refresh(stats.MaxPopulation(), stats.PopulationUsed());
        StatBars[1].Refresh(stats.MaxProduce(), stats.ProduceUsed());
        StatBars[2].Refresh(stats.MaxIndustry(), stats.IndustryUsed());
    }

    bool IsTroopAttackable(Troop troop){
        if(!ValidateTroop(troop))
            return false;

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
        if(ValidateTroop(troop) && ValidateTroop(current_troop))
            _GameplayManager.RPC_AttackTroop(current_troop.UniqueID, troop.UniqueID, true);
        Deselect();
    }

    void SelectTroop(Troop troop){

        if(!ValidateTroop(troop))
            return;

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
        TroopInfoDisplay.SetActive(true);
        TroopName.text = troop.Name;
        TroopName_BG.color = _SessionManager.PlayerFaction(troop.Owner).Colour();
        DisplayBG.color = _SessionManager.PlayerFaction(troop.Owner).Colour();
        
        if(troop.Owner == _SessionManager.OurInstance.ID){
            GetTroopRanges(troop);
        }

        PlaySFX("Pickup", SFX_Lookup);
    }

    public void AddTroop(Troop troop){
        if(!ValidateTroop(troop))
            return;

        if(troop.Owner == _SessionManager.OurInstance.ID){
            OurTroops.Add(troop);
            troop.SetEventCamera(_Camera);
        }
    }

    public void Deselect(){
        CloseSpawnMenu();
        UpgradeWindow.SilentClose();
        UpgradeButton.SetActive(false);
        TroopInfoDisplay.SetActive(false);
        FortStats.SetActive(false);
        if(current_troop != null)
            current_troop.SetSelected(false);
        current_troop = null;
        current_tile = null;
        walkable_tiles = new List<Tile>();
        attackable_tiles = new List<Tile>();
        ResetSelectionUI();
    }

    public void DisableAllTroops(){
        foreach(Troop troop in OurTroops){
            if(ValidateTroop(troop))
                troop.EndTurn();
        }
    }

    public void EnableAllTroops(){
        foreach(Troop troop in OurTroops){
            print(ValidateTroop(troop));
            if(ValidateTroop(troop)){
                troop.NewTurn();
                troop.EnableConquest(Map.ForeignFortress(troop.current_tile));
            }
        }
    }

    // UI //

    public void LeaderboardButton(){
        LeaderboardWindow.Open();
    }

    public void FortUpgradeButton(){
        UpgradeWindow.Setup(_SessionManager.LocalFactionData().CurrencyFormat(current_tile.stats.UpgradeCost()), this);
        UpgradeWindow.Open();
    }

    public void UpgradeFort(){
        if(CanAfford(current_tile.stats.UpgradeCost()) && current_tile.stats.BelowLevelLimit()){
            SpendMoney(current_tile.stats.UpgradeCost());
            Map.RPC_RequestFortLevel(current_tile.ID, current_tile.stats.level + 1);
            Deselect();
        }
    }

    public void UpdateTurnNameDisplay(string name){
        TurnNameText.text = name;
        TurnNameAnim.Play("Fadeout", -1, 0);
    }

    void ResetSelectionUI(){
        TileInfoDisplay.SetActive(false);
        TroopInfoDisplay.SetActive(false);
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

    void GenerateRenderers(){

        RenderTextureDescriptor desc = new RenderTextureDescriptor(177, 256, RenderTextureFormat.ARGB32);

        TroopData[] troops = _SessionManager.LocalFactionData().Troops();
        PieceData[] buildings = _SessionManager.LocalFactionData().Buildings();
        troop_renders = new PreviewRenderer[troops.Length];
        building_renders = new PreviewRenderer[buildings.Length];
    
        for(int count = 0; count < troops.Length; count++){
            PreviewRenderer new_render = GameObject.Instantiate(PreviewRendererPrefab, Vector3.zero, Quaternion.identity).GetComponent<PreviewRenderer>();
            troop_renders[count] = new_render;
            new_render.Setup(TroopRendererHolder, TroopButtonHolder, count, desc, this, _SessionManager.LocalFactionData().Colour(), null, _SessionManager.LocalFactionData().Troops()[count], _TechTreeManager);
            new_render.transform.SetParent(PreviewRendererHolder);
        }

        for(int count = 0; count < buildings.Length; count++){
            PreviewRenderer new_render = GameObject.Instantiate(PreviewRendererPrefab, Vector3.zero, Quaternion.identity).GetComponent<PreviewRenderer>();
            building_renders[count] = new_render;
            new_render.Setup(BuildingRendererHolder, BuildingButtonHolder, count, desc, this, _SessionManager.LocalFactionData().Colour(), buildings[count], null, _TechTreeManager);
            new_render.transform.SetParent(PreviewRendererHolder);
        }
    }

    // SPAWN MENU //

    public bool CanAfford(int amount){return Money() >= amount;}
    public void SpendMoney(int amount){_SessionManager.SpendMoney(amount);}

    public void SpawnMenuArrows(int difference){
        spawn_menu_offset += difference;
        PlaySFX("UI_2", SFX_Lookup);
        OpenSpawnMenu();
    }

    void ResetSpawnMenu(){
        if(!SpawnMenu.activeSelf){
            spawn_menu_offset = 0;
        }

    }

    void CloseSpawnMenu(){
        if(troop_renders != null){
            for(int i = 0; i < troop_renders.Length; i++)
                troop_renders[i].Disable();
        }
        if(building_renders != null){
            for(int i = 0; i < building_renders.Length; i++)
                building_renders[i].Disable();
        }
        SpawnMenu.SetActive(false);
    }

    void ClampSpawnMenuOffset(int total_items){
        int max_offset = ((total_items + 3) / 4) - 1;
        if(spawn_menu_offset < 0)
            spawn_menu_offset = max_offset;
        else if(spawn_menu_offset > max_offset)
            spawn_menu_offset = 0;
    }

    int DisplayedItemCount(int total, int offset){
        int displayed_count = total - offset * 4;
        if(displayed_count > 4)
            displayed_count = 4;
        return displayed_count;
    }

    bool current_spawn_menu_type;
    void OpenSpawnMenu(){OpenSpawnMenu(current_spawn_menu_type);}
    void OpenSpawnMenu(bool troop_building){

        ResetSpawnMenu(); 
        CloseSpawnMenu();

        current_spawn_menu_type = troop_building;

        if(troop_building)
            SetupSpawnButtons(building_renders);
        else
            SetupSpawnButtons(troop_renders);

        SpawnMenu.SetActive(true);
    }

    public void SetupSpawnButtons(PreviewRenderer[] prs){
        float offy = 95;
        int active_count = 0;
        int total_items = 0;

        foreach(PreviewRenderer pr in prs){
            if(pr.Unlocked())
                total_items++;
        }
        
        ClampSpawnMenuOffset(total_items);
        int start_point = spawn_menu_offset * 4;
        int displayed_count = DisplayedItemCount(total_items, spawn_menu_offset);
        float centering = (-1 * offy * displayed_count) / 2;
        centering += (offy / 2);
        
        //BuildingValid(tile, building_renders[i].GetPieceData())

        for(int i = start_point; i < prs.Length && i < start_point + 4; i++){
            if(prs[i].Unlocked()){
                prs[i].SetAfford(Money());
                prs[i].SetPosition(new Vector2((active_count * offy) + centering, 0));
                prs[i].SetTile(current_tile);
                prs[i].Enable();
                active_count++;
            }
        }        

        SpawnMenuArrowsHolder.SetActive(total_items > 4);
    }

    public void SpawnModelHolderTroop(int troop, Transform holder){
        TroopData _troop = _SessionManager.LocalFactionData().Troops()[troop]; // Assume faction is the local player
        int fact = _SessionManager.LocalFactionID();
        SpawnModelHolderTroop(_troop, holder, fact);
    }

    public void SpawnModelHolderBuildng(int building, Transform holder){
        PieceData _building = _SessionManager.LocalFactionData().Buildings()[building]; // Assume faction is the local player
        GameObject b = GameObject.Instantiate(_building.Prefab(), holder.position, Quaternion.identity);
        b.transform.parent = holder;
        SetLayer(b, HiddenLayer);
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
        SpawnInfoPopup.gameObject.SetActive(false);
        for (int index = 0; index < eventSystemRaysastResults.Count; index++){
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer){
                HoverableDisplay(curRaysastResult.gameObject);
                return true;
            }
        }
        return false;
    }

    void HoverableDisplay(GameObject obj){
        SpawnButton butt = null;

        if(obj.tag == "Hoverable"){
            butt = obj.transform.parent.parent.GetComponent<SpawnButton>();
            if(butt != null){
                if(butt.IsTroop())
                    SpawnInfoPopup.Refresh(_SessionManager.LocalFactionData().Troops()[butt.Reference()], current_tile.stats, _SessionManager.LocalFactionData(), Money());
                else
                    SpawnInfoPopup.Refresh(butt.Piece(), Money(), _SessionManager.LocalFactionData());
                SpawnInfoPopup.gameObject.SetActive(true);
            }
        }
    }

    static List<RaycastResult> GetEventSystemRaycastResults(){
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults;
    }

    public int Money(){
        return _SessionManager.OurInstance.Money();
    }

    // TECH TREE //

    public void OpenTechTree(){
        Deselect();
        _TechTreeManager.OpenUI();
        pause_inputs = true;
    }

    public void CloseTechTree(){
        Deselect();
        _TechTreeManager.CloseUI();
        pause_inputs = false;
    }

    // CAMERA //

    void CameraLogic(){
        if(pause_inputs)
            return;

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

        if(Input.GetKeyDown("e")){
            target_spine_rot += 45;
            PlaySFX("Camera_Pivot", SFX_Lookup);
        }
        if(Input.GetKeyDown("q")){
            target_spine_rot += -45;
            PlaySFX("Camera_Pivot", SFX_Lookup);
        }
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
