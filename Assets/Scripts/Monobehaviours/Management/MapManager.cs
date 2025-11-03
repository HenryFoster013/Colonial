using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using Debug=UnityEngine.Debug;
using Fusion;
using MapUtils;
using static GenericUtils;

public class MapManager : NetworkBehaviour
{
    [Header(" - Main - ")]
    [SerializeField] SessionManager _SessionManager;
    [SerializeField] GameplayManager _GameplayManager;
    [SerializeField] SoundEffectLookup SFX_Lookup;
    public bool AnimatedWater = true;

    [Header(" - Map - ")]
    public int MapSize = 16;
    [SerializeField] Transform TileHolder;
    [SerializeField] Transform ColliderHolder;
    [SerializeField] Transform PieceHolder;
    [SerializeField] Transform BackgroundHolder;

    [Header(" - Tiles - ")]
    [SerializeField] TileLookup _TileLookup;
    [SerializeField] PieceLookup _PieceLookup;
    [SerializeField] FactionLookup _FactionLookup;
    
    [Header(" - Mesh Combination - ")]
    [SerializeField] GameObject BackgroundPiece;
    [SerializeField] GameObject HexagonalCollider;
    [SerializeField] GameObject MeshHolder;
    [SerializeField] Material BG_Material;
    [SerializeField] Material WaterMaterial;
    [SerializeField] Material SandMaterial;
    [SerializeField] Material GrassMaterial;
    [SerializeField] Material StoneMaterial;

    [Header(" - Border Generation - ")]
    [SerializeField] Transform BorderHolder;
    [SerializeField] Material BorderMaterial;
    [SerializeField] GameObject[] BorderPrefabs;

    [Header(" - Misc - ")]
    [SerializeField] GameObject BuildingEffect;

    // Local only
    public bool ready {get; private set;}
    public float GrassLimit {get; private set;}
    float StoneLimit = 0.5f;
    bool generated_bg;

    Tile[] Tiles;
    Tile[] city_tiles;
    [HideInInspector] public Seed seed;

    // Synced from Host
    public float[] map_data_raw;
    int[] tile_pieces;
    
    List<MeshFilter> grass_mesh = new List<MeshFilter>();
    List<MeshFilter> sand_mesh = new List<MeshFilter>();
    List<MeshFilter> water_mesh = new List<MeshFilter>();
    List<MeshFilter> stone_mesh = new List<MeshFilter>();
    List<Transform> faction_border_holders = new List<Transform>();
    List<MeshFilter> bg_mesh = new List<MeshFilter>();

    // MAIN FUNCTIONS //

    void Update(){
        AnimateMap();
    }

    // OUTSIDE INTERACTION //

    public Tile GetTile(int i){
        if(!ValidateTileID(i))
            return null;
        else
            return Tiles[i];
    }

    public bool TilesAreNeighbors(int tile_one, int tile_two){return TilesAreNeighbors(Tiles[tile_one], Tiles[tile_two]);}
    public bool TilesAreNeighbors(Tile tile_one, Tile tile_two){
        List<Tile> neighbors = TilesByDistance(tile_one, 1, false);
        return neighbors.Contains(tile_two);
    }

    public List<Tile> TilesByDistance(int id, int distance, bool walkable_only){return TilesByDistance(Tiles[id], distance, walkable_only);}
    public List<Tile> TilesByDistance(Tile origin_tile, int distance, bool walkable_only){
        List<Tile> result = new List<Tile>();
        List<Tile> checked_tiles = new List<Tile>();
        List<Tile> last_iteration = new List<Tile>();
        List<Tile> last_iteration_buffer = new List<Tile>();
        
        last_iteration.Add(origin_tile);

        for(int i = 0; i < distance; i++){

            last_iteration_buffer = new List<Tile>(last_iteration);
            last_iteration = new List<Tile>();

            foreach(Tile tile in last_iteration_buffer){
                foreach(Tile neighbor in GetNeighbors(tile)){
                    if(!checked_tiles.Contains(neighbor)){   
                        if((walkable_only && CheckWalkable(neighbor)) || !walkable_only){
                            result.Add(neighbor);
                            last_iteration.Add(neighbor);
                        }
                        checked_tiles.Add(neighbor);
                    }
                }
            }
        }

        // In theory shouldn't be needed, uncomment if duplicate Tiles appear.
        //result = result.Distinct().ToList();
        
        return result;
    }


    public bool CheckWalkable(Tile tile){
        return tile.type.Walkable() && tile.piece.Walkable();
    }

    List<Tile> WalkableFilter(List<Tile> Tiles){
        List<Tile> result = new List<Tile>();
        foreach(Tile tile in Tiles){
            if(CheckWalkable(tile) && !_GameplayManager.TroopOnTile(tile))
                result.Add(tile);
        }
        return result;
    }

    public Vector3 CalcTileWorldPosition(Tile tile){return CalcTileWorldPosition(tile.ID);}
    public Vector3 CalcTileWorldPosition(int tile_id){
        
        float height = (map_data_raw[tile_id]) / 2;
        Vector3 return_val = Vector3.zero;
        Vector2Int tile_coords = TileToCoords(tile_id);
        int y = tile_coords.y;
        int x = tile_coords.x;
        float ybounce = 0f;
        if(tile_id % 2 != 0)
            ybounce = 0.5f;
        return_val = new Vector3((tile_coords.x * 0.75f), height, tile_coords.y + ybounce);

        if(Tiles[tile_id] != null){
            if(Tiles[tile_id].type == _TileLookup.Tile("WATER"))
                return_val = CalcTileWaterHeight(return_val);
        }
        
        return return_val;
    }

    public Vector3 CalcTileWaterHeight(Vector3 pos){
        float offset = -1.8f;
        offset += Mathf.Sin((Time.time * .5f) + pos.x);
        offset += Mathf.Sin((Time.time * .75f) + pos.z);

        return new Vector3(pos.x, Mathf.Clamp(pos.y + (offset / 8f), -0.99f, pos.y + 0.2f), pos.z);
    }

    Vector2Int TileToCoords(Tile tile){return TileToCoords(tile.ID);}
    Vector2Int TileToCoords(int id){
        int y = id / MapSize;
        int x = id - (MapSize * y);
        return (new Vector2Int(x, y));
    }

    public Vector3 GetTroopPosition(int tile_id){return GetTroopPosition(Tiles[tile_id]);}
    public Vector3 GetTroopPosition(Tile tile){
        return tile.world_position + new Vector3(0, tile.piece.TroopOffset(), 0);
    }

    // Works backwards from the in world position. The math should be much faster than searching a list of all Tiles.
    public Tile GetTileFromTransform(Transform t){
        float grid_x = (t.position.x) / 0.75f;

        float bounceval = 0;
        if(Mathf.Round(Mathf.Round(grid_x) % 2) != 0)
            bounceval = 0.5f;

        float grid_y = (t.position.z - bounceval);
        
        int y_int = (int)(Mathf.Round(grid_y));
        int x_int = (int)(Mathf.Round(grid_x));

        return Tiles[GetTileID(new Vector2Int(x_int, y_int))];
    }

    public int GetTileID(Vector2Int pos){
        int tile = 0;
        if(pos.x >= 0 && pos.y >= 0 && pos.x < MapSize && pos.y < MapSize)
            tile = pos.x + (MapSize * pos.y);
        if(tile >= map_data_raw.Length || tile < 0)
            tile = 0;

        return tile;
    }

    public List<Tile> GetNeighbors(Tile tile){

        List<Tile> neighbors = new List<Tile>();
        if(AntiLoopingMeasures(tile, +1, 0)) {neighbors.Add(Tiles[tile.ID + 1]);}
        if(AntiLoopingMeasures(tile, -1, 0)) {neighbors.Add(Tiles[tile.ID - 1]);}
        if(ValidateTileID(tile.ID + MapSize)) {neighbors.Add(Tiles[tile.ID + MapSize]);}
        if(ValidateTileID(tile.ID - MapSize)) {neighbors.Add(Tiles[tile.ID - MapSize]);}

        if(tile.ID % 2 == 0){
            if(ValidateTileID(tile.ID + 1 - MapSize) && AntiLoopingMeasures(tile, +1, -1)) {neighbors.Add(Tiles[tile.ID + 1 - MapSize]);}
            if(ValidateTileID(tile.ID - 1 - MapSize) && AntiLoopingMeasures(tile, -1, -1)) {neighbors.Add(Tiles[tile.ID - 1 - MapSize]);}
        }
        else{
            if(ValidateTileID(tile.ID + 1 + MapSize) && AntiLoopingMeasures(tile, +1, +1)) {neighbors.Add(Tiles[tile.ID + 1 + MapSize]);}
            if(ValidateTileID(tile.ID - 1 + MapSize) && AntiLoopingMeasures(tile, -1, +1)) {neighbors.Add(Tiles[tile.ID - 1 + MapSize]);}
        }
        return neighbors;
    }

    bool AntiLoopingMeasures(Tile tile, int x_offset, int y_offset){return AntiLoopingMeasures(tile.ID, x_offset, y_offset);}
    bool AntiLoopingMeasures(int tile, int x_offset, int y_offset){
        if(!ValidateTileID(tile))
            return false;
        
        int new_tile = tile + x_offset + (y_offset * MapSize);

        if(!ValidateTileID(new_tile))
            return false;

        Vector2Int base_tile_coords = TileToCoords(Tiles[tile]) + new Vector2Int(x_offset, y_offset);
        Vector2Int new_tile_coords = TileToCoords(Tiles[new_tile]);
        
        return base_tile_coords == new_tile_coords;
    }

    public bool ValidateTileID(int pos){
        return(pos < map_data_raw.Length && pos > -1);
    }

    // MAP ANIMATION //

    void AnimateMap(){
        if(!AnimatedWater || !ready)
            return;
        
        foreach(Tile tile in Tiles){
            if(tile.type.CheckType("WATER") && tile.visible && tile.created){
                
                tile.SetPosition(CalcTileWorldPosition(tile));
                tile.water_transform.localScale = new Vector3(tile.water_transform.localScale.x, tile.water_transform.localScale.x * (tile.world_position.y + 1), tile.water_transform.localScale.z);

                if(tile.piece_transform != null)
                    tile.piece_transform.position = tile.world_position;
            }
        }
    }

    // MAP GENERATION //

    public void EstablishOtherRandoms(){
        CreateTiles(false);

        // Towers pass
        PlaceTowers(_SessionManager.GetPlayerCount());
        OwnershipVisibilityPass();
        
        // Extras pass
        ExtrasPass();
        RecalculateTotalValue();
    }

    public void GenerateMap(){
        GenerateVisibleMapMesh();
        SetupFactionBorders();
    }

    void GenerateVisibleMapMesh(){

        Stopwatch st = new Stopwatch();
        st.Start();

        // Tile creation
        foreach(Tile tile in Tiles){

            Vector3 pos = new Vector3(tile.world_position.x, 0, tile.world_position.z);

            if(!generated_bg){
                GameObject bg_tile = GameObject.Instantiate(BackgroundPiece, pos, Quaternion.identity);
                bg_mesh.Add(bg_tile.transform.GetChild(0).GetComponent<MeshFilter>());
            }

            if(tile.visible && !tile.created){
                tile.Created();


                GameObject tile_obj = GameObject.Instantiate(tile.type.Prefab(), pos, Quaternion.identity);
                tile_obj.transform.parent = TileHolder;
                tile_obj.transform.localScale = new Vector3(tile_obj.transform.localScale.x, tile_obj.transform.localScale.y * (tile.world_position.y + 1), tile_obj.transform.localScale.z);

                if(!AnimatedWater || AnimatedWater && !tile.type.CheckType("WATER")){
                    GameObject collider = GameObject.Instantiate(HexagonalCollider, pos, Quaternion.identity);
                    collider.transform.localScale = new Vector3(collider.transform.localScale.x, collider.transform.localScale.y * (tile.world_position.y + 1), collider.transform.localScale.z);
                    collider.transform.parent = ColliderHolder;
                }

                // Track meshes and their types
                switch(tile.type.Type()){
                    case "WATER":
                        if(!AnimatedWater)
                            water_mesh.Add(tile_obj.transform.GetChild(0).GetComponent<MeshFilter>());
                        else
                            tile.SetWaterTransform(tile_obj.transform);
                        break;
                    case "GRASS":
                        grass_mesh.Add(tile_obj.transform.GetChild(0).GetComponent<MeshFilter>());
                        break;
                    case "SAND":
                        sand_mesh.Add(tile_obj.transform.GetChild(0).GetComponent<MeshFilter>());
                        break;
                    case "STONE":
                        stone_mesh.Add(tile_obj.transform.GetChild(0).GetComponent<MeshFilter>());
                        break;
                }

                GeneratePieceModel(tile);
            }
        }

        CombineMapMeshes();

        st.Stop();
        Debug.Log(string.Format("New map generated in {0} ms", st.ElapsedMilliseconds));

        ready = true;
    }

    void SetupFactionBorders(){
        for(int owner = 0; owner < _FactionLookup.Length(); owner++){
            GameObject g = GameObject.Instantiate(BorderHolder.gameObject);
            g.transform.parent = BorderHolder;
            faction_border_holders.Add(g.transform);
            RefreshBorderMesh(owner);
        }
    }

    void CombineMapMeshes(){
        if(!AnimatedWater){
            CombineMeshes(ref water_mesh, WaterMaterial, TileHolder);
        }
        CombineMeshes(ref grass_mesh, GrassMaterial, TileHolder);
        CombineMeshes(ref sand_mesh, SandMaterial, TileHolder);
        CombineMeshes(ref stone_mesh, StoneMaterial, TileHolder);

        if(!generated_bg){
            generated_bg = true;
            CombineMeshes(ref bg_mesh, BG_Material, BackgroundHolder);
        }
    }

    void CombineMeshes(ref List<MeshFilter> meshes, Material mat, Transform parenty){
        CombineInstance[] instances = new CombineInstance[meshes.Count];

        for (int i = 0; i < meshes.Count; i++){
            var meshFilter = meshes[i];
            
            instances[i] = new CombineInstance{
                mesh = meshFilter.sharedMesh,
                transform = meshFilter.transform.localToWorldMatrix,
            };

            Destroy(meshFilter.transform.parent.gameObject);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(instances, true);
        GameObject mesh_holder = GameObject.Instantiate(MeshHolder);
        MeshFilter mf = mesh_holder.transform.GetChild(0).GetComponent<MeshFilter>();
        mf.sharedMesh = combinedMesh;
        meshes = new List<MeshFilter>();
        meshes.Add(mf);
        mesh_holder.transform.GetChild(0).GetComponent<MeshRenderer>().material = mat;
        mesh_holder.transform.parent = parenty;
    }

    public void ClientGenerateMap(){
        CreateTiles(true);
        OwnershipVisibilityPass();
        GenerateMap();
    }

    List<TileStats> towers_forts_stats = new List<TileStats>();
    public void OwnershipVisibilityPass(){
        _FactionLookup.ShuffleLocationNames(seed.RandomInt(), seed.RandomInt());
        foreach(Tile tile in Tiles){
            if(tile.piece.CheckType("Tower")){
                TileStats stats = new TileStats(tile, "temp", 5);
                if(tile.piece == _SessionManager.OurInstance.FactionData().Tower()){
                    _SessionManager.OurInstance.SnapCameraToPosition(CalcTileWorldPosition(tile) + new Vector3(8,0,0));
                    MarkRadiusAsVisible(tile, stats.ownership_radius);
                }
                MarkRadiusAsOwned(tile, stats.ownership_radius, tile.piece.Owner(), true);
                stats.SetName(TileToLocationName(tile));
                stats.RefreshDetails(this);
                towers_forts_stats.Add(stats);
            }
        }
    }

    public void RefreshAllCities(){
        foreach(TileStats stats in towers_forts_stats){
            stats.RefreshDetails(this);
        }
        RecalculateTotalValue();
    }

    public string TileToLocationName(Tile tile){
        return tile.owner.LocationNameset().GetLocationName(tile.ID);
    }

    public void CreateTiles(bool client){
        Tiles = new Tile[map_data_raw.Length];
        for(int i = 0; i < map_data_raw.Length; i++){
            PieceData piece = _PieceLookup.Piece("UNMARKED");
            Tile new_tile = new Tile(i, map_data_raw[i], GetRawType(map_data_raw[i]), piece, CalcTileWorldPosition(i), null);
            Tiles[i] = new_tile;
        }

        foreach(Tile t in Tiles){
            if(t.type.CheckType("WATER")){
                foreach(Tile t_ in GetNeighbors(t)){
                    if(t_.type.CheckType("GRASS"))
                        t_.SetType(_TileLookup.Tile("SAND"));
                }
            }
        }
    }

    TileData GetRawType(float raw){
        if(raw > StoneLimit)
            return _TileLookup.Tile("STONE");
        else if(raw > GrassLimit)
            return _TileLookup.Tile("GRASS");
        else
            return _TileLookup.Tile("WATER");
    }

    bool IsDistanceFromEdge(Vector2Int our_coords, int dist){
        return (our_coords.x > dist && our_coords.x < MapSize - dist && our_coords.y > dist && our_coords.y < MapSize - dist);
    }

    void PlaceTowers(int castles_needed){
        // Use of a minimum distance that gets lower as the failed attempts increase
        // Ensure that the castles are placed randomly but far enough apart that the game is fair
        // But if no such position can be found quickly, the distance needed is reduced until it can be

        List<Vector2Int> placed_castles = new List<Vector2Int>();

        int minimum_distance = MapSize * 2;
        int distance_fails = 0;
        int forts_needed = (MapSize / 4) - castles_needed;

        city_tiles = new Tile[castles_needed + forts_needed];

        while(placed_castles.Count < castles_needed + forts_needed){

            if(distance_fails > 15){
                minimum_distance -= 1;
                distance_fails = 0;
            }

            Tile check_tile = Tiles[seed.RangeInt(0, MapSize * MapSize)];
            Vector2Int our_coords = TileToCoords(check_tile);

            if(IsDistanceFromEdge(our_coords, 3)){
                if(check_tile.piece == _PieceLookup.Piece("UNMARKED")){ // Empty
                    if(check_tile.type.CheckType("GRASS") || check_tile.type.CheckType("SAND")){

                        bool valid = true;
                        foreach(Vector2Int pos in placed_castles){
                            if((our_coords - pos).sqrMagnitude < minimum_distance){
                                valid = false;
                            }
                        }

                        if(valid){
                            if(placed_castles.Count < castles_needed){ // Place Capital
                                PlayerInstance player = _SessionManager.GetPlayer(placed_castles.Count);
                                Faction _owner = player.FactionData();
                                check_tile.SetPiece(player.FactionData().Tower());
                            }
                            else{ // Place Fort
                                check_tile.SetPiece(_PieceLookup.Piece_ByName("Fort (Empty)"));
                            }
                            city_tiles[placed_castles.Count] = check_tile;
                            placed_castles.Add(TileToCoords(check_tile));
                        }
                        else{
                            distance_fails++;
                        }
                    }
                }
            }
        }
    }

    public void CheckForMapRegen(){
        GenerateVisibleMapMesh();
        RefreshAllBorders();
    }

    // TILE OWNERSHIP //

    int total_value;
    public int TotalValue(){return total_value;}
    public void RecalculateTotalValue(){
        total_value = 0;
        if(city_tiles == null)
            return;
        
        int return_val = 0;
        for(int i = 0; i < city_tiles.Length; i++){
            if(city_tiles[i] != null){
                if(city_tiles[i].stats != null){
                    if(city_tiles[i].owner == _SessionManager.OurInstance.FactionData()){
                        return_val += city_tiles[i].stats.Value();
                    }
                }
            }
        }
        
        total_value = return_val;
    }

    public void CleanCities(){
        for(int i = 0; i < city_tiles.Length; i++){
            if(city_tiles[i].stats != null)
                city_tiles[i].stats.ReleaseUsage();
        }
        RecalculateTotalValue();
    }

    public void AddTroopStats(Troop troop){
        if(troop == null)
            return;
        if(!troop.spawned)
            return;
        if(troop.FactionData() != Tiles[troop.HomeTile].owner)
            return;
        Tiles[troop.HomeTile].stats.AddTroopStats(troop.Data);
    }

    public bool ForeignFortress(int id){return ForeignFortress(Tiles[id]);}
    public bool ForeignFortress(Tile tile){        
        bool valid = false;
        if(tile.owner != _SessionManager.OurInstance.FactionData()){
            if(IsTileFortress(tile))
                valid = true;
        }
        
        return valid;
    }

    public void Conquer(Tile tile, Faction owner){
        if(!_SessionManager.Hosting)
            return;
        
        if(IsTileFortress(tile)){
            if(tile.piece.CheckType("Tower")){
                RPC_PieceChanged(tile.ID, _PieceLookup.ID(owner.Tower()), false);
            }
            else{
                RPC_PieceChanged(tile.ID, _PieceLookup.ID(owner.Fort()), false);
            }

            RPC_Conquer(tile.ID, _FactionLookup.ID(owner));
        }
    }

    void MarkRadiusAsOwned(Tile tile, int radius, Faction owner, bool do_not_overwrite){
        MarkTileAsOwned(tile, owner, do_not_overwrite);
        tile.stats.SetOwnershipRadius(radius);
        foreach(Tile tile_ in TilesByDistance(tile, radius, false)){
            MarkTileAsOwned(tile_, owner, do_not_overwrite);
        }

        if(owner == _SessionManager.OurInstance.FactionData()){
            MarkRadiusAsVisible(tile, radius);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PieceChanged(int tile_id, int piece_id, bool play_effect, RpcInfo info = default){
        PieceData piece = _PieceLookup.Piece(piece_id);
        Tiles[tile_id].SetPiece(piece);
        PlayBuildingEffect(Tiles[tile_id], piece, play_effect);
        GeneratePieceModel(Tiles[tile_id]);
        RefreshAllCities();
    }

    void PlayBuildingEffect(Tile tile, PieceData piece, bool truth){

        // Any return conditions here, these are piece swaps we do not want the effect to spawn for (eg hunting sharks)
        if(!truth)
            return;
        if(IsTileFortress(tile))
            return;
        if(!tile.visible)
            return;
            
        if(piece.PlayConstructionSound())
            PlaySFX("Build_Hammers", SFX_Lookup);
        SpawnParticleEffect(tile);
    }


    public void SpawnParticleEffect(int tile){SpawnParticleEffect(GetTile(tile));}
    public void SpawnParticleEffect(Tile tile){
        GameObject.Instantiate(BuildingEffect, tile.world_position, Quaternion.identity);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestFortLevel(int tile_id, int level, RpcInfo info = default){
        // Validate here
        if(level == Tiles[tile_id].stats.level + 1){
            RPC_SetFortLevel(tile_id, level);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetFortLevel(int tile_id, int level, RpcInfo info = default){
        print(Tiles[tile_id].stats.BelowLevelLimit());

        if(!Tiles[tile_id].stats.BelowLevelLimit())
            return;

        Tiles[tile_id].stats.SetLevel(level);
        MarkRadiusAsOwned(Tiles[tile_id], Tiles[tile_id].stats.ownership_radius, Tiles[tile_id].owner, true);
        RefreshAllBorders();
        Tiles[tile_id].stats.RefreshDetails(this);
        CheckForMapRegen();
    }

    void ConquestSFX(Tile tile, Faction faction){
        if(tile.visible){
            if(faction != null){
                if(faction != tile.owner)
                    PlaySFX(faction.Jingle());
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Conquer(int tile_id, int owner_id, RpcInfo info = default){
        int radius = 2;
        TileStats stats = Tiles[tile_id].stats;
        Faction faction = _FactionLookup.GetFaction(owner_id);

        ConquestSFX(Tiles[tile_id], faction);
        
        if(stats == null){
            stats = new TileStats(Tiles[tile_id], "temp", 3);
            towers_forts_stats.Add(stats);
        }
        else
            radius = stats.ownership_radius;

        MarkRadiusAsOwned(Tiles[tile_id], radius, faction, false);
        RefreshAllBorders();
        stats.SetName(TileToLocationName(Tiles[tile_id]));
        stats.RefreshDetails(this);
        RecalculateTotalValue();
    }

    void MarkTileAsOwned(Tile tile, Faction owner, bool do_not_overwrite){
        if(tile.owner == null && do_not_overwrite || !do_not_overwrite)
            tile.SetOwner(owner);
    }

    public void MarkRadiusAsVisible(Tile tile, int radius){
        tile.Visible();
        foreach(Tile tile_ in TilesByDistance(tile, radius, false)){
            tile_.Visible();
        }
    }

    void RefreshAllBorders(){
        for(int i = 0; i < _FactionLookup.Length(); i++)
            RefreshBorderMesh(i);
    }

    void RefreshBorderMesh(int owner_id){
        Transform border_holder = faction_border_holders[owner_id];

        foreach(Transform t in border_holder)
            Destroy(t.gameObject);

        List<MeshFilter> border_pieces = new List<MeshFilter>();

        foreach(Tile tile in Tiles){
            if(tile.owner == _FactionLookup.GetFaction(owner_id))
                border_pieces.AddRange(PlaceNewBorders(tile.ID, owner_id, border_holder));
        }

        Material mat = new Material(BorderMaterial);
        mat.SetColor("_BaseColour", _FactionLookup.GetFaction(owner_id).BorderColour());

        CombineMeshes(ref border_pieces, mat, border_holder);
    }

    List<MeshFilter> border_buffer = new List<MeshFilter>();
    List<MeshFilter> PlaceNewBorders(int tile, int owner_id, Transform bh){
        
        border_buffer = new List<MeshFilter>();
        
        if(!Tiles[tile].visible || !Tiles[tile].created)
            return border_buffer;

        CreateBorder(tile, tile + MapSize, bh, owner_id, 1);
        CreateBorder(tile, tile - MapSize, bh, owner_id, 4);
        if(tile % 2 == 0){ // Down Tile
            CreateBorder(tile, tile + 1, bh, owner_id, 2); //TR
            CreateBorder(tile, tile - 1, bh, owner_id, 6); //TL
            CreateBorder(tile, tile - MapSize - 1, bh, owner_id, 5); // BL
            CreateBorder(tile, tile - MapSize + 1, bh, owner_id, 3); // BR
        }
        else{ // Up Tile
            CreateBorder(tile, tile + 1, bh, owner_id, 3); //BR
            CreateBorder(tile, tile - 1, bh, owner_id, 5); //BL
            CreateBorder(tile, tile + MapSize - 1, bh, owner_id, 6); // TL
            CreateBorder(tile, tile + MapSize + 1, bh, owner_id, 2); // TR
        }

        return border_buffer;
    }

    void CreateBorder(int tile_id, int comp_tile, Transform border_holder, int owner_id, int prefab){
        prefab = prefab - 1;

        bool border_here = false;
        if(ValidateTileID(comp_tile)){
            if(_FactionLookup.ID(Tiles[comp_tile].owner) != owner_id){
                border_here = true;
            }
        }
        else{
            border_here = true;
        }

        if(border_here && Tiles[tile_id].created){
            Vector3 pos = CalcTileWorldPosition(tile_id);
            pos = new Vector3(pos.x, 0, pos.z);
            GameObject border_obj = GameObject.Instantiate(BorderPrefabs[prefab], pos, Quaternion.identity);
            border_obj.transform.parent = border_holder;
            border_buffer.Add(border_obj.transform.GetChild(0).GetComponent<MeshFilter>());
        }
    }

    // PIECE PLACING //

    void ExtrasPass(){

        foreach(Tile tile in Tiles){

            if(tile.piece.CheckType("UNMARKED")){

                // Grass fill
                if(tile.type.CheckType("GRASS")){
                    if(seed.Range(0.2f, 1f) + tile.raw >= 1){
                        CoinFlipPiece(tile, "Tree Large", "Tree Small");
                    }

                    RandomChancePiece(tile, 40, "Piggie");
                    RandomChancePiece(tile, 30, "Apples");
                    RandomChancePiece(tile, 40, "Piggie (Grass)");
                    RandomChancePiece(tile, 5, "Tall Grass");
                }

                // Sand fill
                if(tile.type.CheckType("SAND")){
                    RandomChancePiece(tile, 20, "Palm Tree");
                    RandomChancePiece(tile, 35, "X Mark");
                }
                
                // Stone fill
                if(tile.type.CheckType("STONE")){
                    if(seed.Range(0f, 0.5f) + tile.raw >= 1){
                        tile.SetPiece(_PieceLookup.Piece("MOUNTAIN"));
                    }
                }

                // Ocean fill
                if(tile.type.CheckType("WATER")){
                    RandomChancePiece(tile, 50, "Sharkfin");
                }
            }
        }
    }

    void RandomChancePiece(Tile tile, int odds, string piece){
        if(seed.RangeInt(0, odds + 1) == 0)
            tile.SetPiece(_PieceLookup.Piece(piece));
    }

    void CoinFlipPiece(Tile tile, string piece_a, string piece_b){
        if(seed.RangeInt(0, 3) == 0)
            tile.SetPiece(_PieceLookup.Piece(piece_a));
        else
            tile.SetPiece(_PieceLookup.Piece(piece_b));
    }

    void GeneratePieceModel(Tile tile){

        if(tile.piece.CheckType("UNMARKED") || !tile.created || !tile.visible)
            return;

        if(tile.piece_transform != null){
            GameObject.Destroy(tile.piece_transform.gameObject);
        }

        GameObject g = GameObject.Instantiate(tile.piece.Prefab(), tile.world_position, Quaternion.identity);
        if(tile.piece.RandomRotation())
            g.transform.eulerAngles = new Vector3(0, seed.Range(0f, 360f), 0f);
        if(tile.piece.RandomChildRotation())
            g.transform.GetChild(0).eulerAngles = new Vector3(0, seed.Range(0f, 360f), 0f);
        tile.SetPieceTransform(g.transform);
        g.transform.parent = PieceHolder;
    }

    // NOISE GENERATION //

    public void EstablishNoiseMap(){
        NoiseGenerator noise_gen = new NoiseGenerator(seed, MapSize, 3, 12, 300);
        map_data_raw = noise_gen.Generate();
        GrassLimit = seed.Range(-0.2f, -0.6f);
    }

    // GETTERS //

    public bool CheckVisibility(int tile){return Tiles[tile].visible;}
    public Tile[] GetCities(){return city_tiles;}

    public bool IsOwner(Tile tile, Faction owner){
        return tile.owner == owner;
    }
}
