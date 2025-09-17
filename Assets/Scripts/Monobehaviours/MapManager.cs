using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using Debug=UnityEngine.Debug;
using Fusion;

public class MapManager : MonoBehaviour
{
    [Header(" - Main - ")]
    [SerializeField] NoiseManager _NoiseManager;
    [SerializeField] Renderer PreviewRenderer;
    [SerializeField] SessionManager _SessionManager;
    public bool AnimatedWater = true;

    [Header(" - Map - ")]
    public const int MapSize = 32;
    [SerializeField] Transform MapHolder;
    [SerializeField] Transform TileHolder;
    [SerializeField] Transform ColliderHolder;
    [SerializeField] Transform PieceHolder;
    [SerializeField] Transform BackgroundHolder;
    [SerializeField] float TileHeightVariation = 0.7f;

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

    // Local only
    bool ready = false;
    public bool Ready(){return ready;}

    public float GrassLimit {get; private set;}
    float StoneLimit = 0.5f;
    bool generated_bg;

    Transform[] piece_transforms;
    bool[] tiles_visible;
    bool[] tiles_created;
    bool[] requires_piece_refresh;
    float[] tile_positions;
    int[] tile_data;
    List<MeshFilter> grass_mesh = new List<MeshFilter>();
    List<MeshFilter> sand_mesh = new List<MeshFilter>();
    List<MeshFilter> water_mesh = new List<MeshFilter>();
    List<MeshFilter> stone_mesh = new List<MeshFilter>();
    List<Transform> player_border_holders = new List<Transform>();
    List<MeshFilter> bg_mesh = new List<MeshFilter>();
    Transform[] water_transforms;

    // Synced from Host
    float[] map_data_raw;
    public float[] GetRawMapData(){return map_data_raw;}

    int[] tile_pieces;
    public int[] GetTilePieces(){return tile_pieces;}

    int[] tiles_owned;
    public int[] GetTileOwnership(){return tiles_owned;}
    

    // MAIN FUNCTIONS //

    void Update(){
        AnimateMap();
    }

    // OUTSIDE INTERACTION //

    public bool TilesAreNeighbors(int tile_one, int tile_two){
        if(!ValidateTile(tile_one) || !ValidateTile(tile_two))
            return false;
        
        List<int> neighbors = TilesByDistance(tile_one, 1, false);
        return neighbors.Contains(tile_two);
    }

    public List<int> TilesByDistance(int origin_tile, int distance, bool walkable_only){
        List<int> result = new List<int>();
        List<int> temp = new List<int>();
        List<int> add = new List<int>();
        if(ValidateTile(origin_tile)){

            // loop thru each item in result, get their neigbors
            // if the neighbor is already in result, remove it from the list
            // add the lists together
            // repeat until distance runs out
            // can optimise by ignoring values that have already been searched

            int starting_point = 0;
            result.Add(origin_tile);

            for(int p = 0; p < distance; p++){
                add = new List<int>();

                for(int i = starting_point; i < result.Count; i++){
                    temp = GetNeighbors(result[i]);

                    foreach(int q in temp){
                        if(!result.Contains(q))
                            add.Add(q);
                    }
                }

                if(walkable_only)
                    result.AddRange(WalkableFilter(add));
                else
                    result.AddRange(add);
            }

            result.Remove(origin_tile);
        }

        // Remove dupes
        result = result.Distinct().ToList();

        return result;
    }

    public int GetTileType(int i){
        return tile_data[i];
    }

    public int GetPieceType(int i){
        return tile_pieces[i];
    }

    public bool CheckWalkable(int i){
        return _TileLookup.Tile(tile_data[i]).Walkable() && _PieceLookup.Piece(tile_pieces[i]).Walkable();
    }

    List<int> WalkableFilter(List<int> tiles){
        List<int> result = new List<int>();
        foreach(int i in tiles){
            if(CheckWalkable(i))
                result.Add(i);
        }
        return result;
    }

    public Vector3 GetTilePosition(int titty){
        Vector2Int tile_coords = TileToCoords(titty);
        
        int y = tile_coords.y;
        int x = tile_coords.x;
        float ybounce = 0f;
        if(titty % 2 != 0)
            ybounce = 0.5f;
        
        return new Vector3((tile_coords.x * 0.75f), tile_positions[titty], tile_coords.y + ybounce);
    }

    public Vector3 GetTroopPosition(int tile){
        return GetTilePosition(tile) + new Vector3(0, _PieceLookup.Piece(GetPieceType(tile)).TroopOffset(), 0);
    }

    public int GetTileID(Vector2Int pos){
        int tile = 0;
        if(pos.x >= 0 && pos.y >= 0 && pos.x < MapSize && pos.y < MapSize)
            tile = pos.x + (MapSize * pos.y);
        if(tile >= map_data_raw.Length || tile < 0)
            tile = 0;

        return tile;
    }

    public int GetSize(){
        return MapSize;
    }

    public List<int> GetNeighbors(int tile){
        List<int> neighbors = new List<int>();
        if(ValidateTile(tile)){
            if(ValidateTile(tile + 1) && AntiLoopingMeasures(tile, +1, 0)) {neighbors.Add(tile + 1);}
            if(ValidateTile(tile - 1) && AntiLoopingMeasures(tile, -1, 0)) {neighbors.Add(tile - 1);}
            if(ValidateTile(tile + MapSize)) {neighbors.Add(tile + MapSize);}
            if(ValidateTile(tile - MapSize)) {neighbors.Add(tile - MapSize);}

            if(tile % 2 == 0){
                if(ValidateTile(tile + 1 - MapSize) && AntiLoopingMeasures(tile, +1, -1)) {neighbors.Add(tile + 1 - MapSize);}
                if(ValidateTile(tile - 1 - MapSize) && AntiLoopingMeasures(tile, -1, -1)) {neighbors.Add(tile - 1 - MapSize);}
            }
            else{
                if(ValidateTile(tile + 1 + MapSize) && AntiLoopingMeasures(tile, +1, +1)) {neighbors.Add(tile + 1 + MapSize);}
                if(ValidateTile(tile - 1 + MapSize) && AntiLoopingMeasures(tile, -1, +1)) {neighbors.Add(tile - 1 + MapSize);}
            }
        }
        return neighbors;
    }

    bool AntiLoopingMeasures(int tile, int x_offset, int y_offset){
        if(!ValidateTile(tile))
            return false;
        
        int new_tile = tile + x_offset + (y_offset * MapSize);

        if(!ValidateTile(new_tile))
            return false;

        Vector2Int base_tile_coords = TileToCoords(tile) + new Vector2Int(x_offset, y_offset);
        Vector2Int new_tile_coords = TileToCoords(new_tile);
        
        return base_tile_coords == new_tile_coords;
    }

    public bool ValidateTile(int pos){
        return(pos < map_data_raw.Length && pos > -1);
    }

    // Works backwards from the in world position. The math should be much faster than searching a list of all tiles.
    public int GetTileIDFromTransform(Transform t){
        float grid_x = (t.position.x) / 0.75f;

        float bounceval = 0;
        if(Mathf.Round(Mathf.Round(grid_x) % 2) != 0)
            bounceval = 0.5f;

        float grid_y = (t.position.z - bounceval);
        
        int y_int = (int)(Mathf.Round(grid_y));
        int x_int = (int)(Mathf.Round(grid_x));

        return GetTileID(new Vector2Int(x_int, y_int));
    }

    // MAP ANIMATION //

    void AnimateMap(){
        if(!AnimatedWater || !ready)
            return;
        
        for(int i = 0; i < water_transforms.Length; i++){
            if(water_transforms[i] != null && tiles_visible[i] && tiles_created[i]){
                float final_vert = tile_positions[i] + (Mathf.Sin((Time.time * .5f) + (water_transforms[i].position.x)) + Mathf.Sin((Time.time * .75f) + (water_transforms[i].position.y))) * 0.1f;

                if(final_vert < -0.99f)
                    final_vert = -0.99f;
                
                Vector3 pos = new Vector3(water_transforms[i].position.x, final_vert, water_transforms[i].position.z);
                water_transforms[i].localScale = new Vector3(water_transforms[i].localScale.x, water_transforms[i].localScale.x * (final_vert + 1), water_transforms[i].localScale.z);

                if(piece_transforms[i] != null)
                    piece_transforms[i].position = pos;
            }
        }
    }

    // MAP GENERATION //

    public void EstablishOtherRandoms(){
        ResetMapData();
        
        // Base pass
        BaseTilePass();

        // Towers pass
        PlaceTowers(_SessionManager.GetPlayerCount());
        VisibleTilesPass();
        
        // Extras pass
        for(int i = 0; i < tile_data.Length; i++){
            CheckTileExtras(i);
        }
    }

    public void BaseTilePass(){
        for(int i = 0; i < map_data_raw.Length; i++){
            MarkTileType(i);
        }

        // Sand pass
        for(int i = 0; i < tile_data.Length; i++){
            MarkSandTiles(i);
        }
    }

    public void GenerateMap(){
        GenerateVisibleMapMesh();
        SetupPlayerBorders();
    }

    void GenerateVisibleMapMesh(){
        
        Stopwatch st = new Stopwatch();
        st.Start();

        int x = 0;
        int y = 0;
        float ybounce = 0f;

        // Tile creation
        for(int i = 0; i < tile_data.Length; i++){

            if(!generated_bg){
                GameObject bg_tile = GameObject.Instantiate(BackgroundPiece, new Vector3((x * 0.75f), 0, y + ybounce), Quaternion.identity);
                bg_mesh.Add(bg_tile.transform.GetChild(0).GetComponent<MeshFilter>());
            }

            if(tiles_visible[i] && !tiles_created[i]){
                tiles_created[i] = true;

                string type = _TileLookup.Tile(tile_data[i]).Type().ToUpper();

                tile_positions[i] = map_data_raw[i] * TileHeightVariation;
                Vector3 decided_position = new Vector3((x * 0.75f), 0, y + ybounce);
                GameObject tile = GameObject.Instantiate(GetTilePrefab(i), decided_position, Quaternion.identity);
                tile.transform.parent = TileHolder;
                tile.transform.localScale = new Vector3(tile.transform.localScale.x, tile.transform.localScale.y * (tile_positions[i] + 1), tile.transform.localScale.z);

                if(!AnimatedWater || AnimatedWater && type != "WATER"){
                    GameObject collider = GameObject.Instantiate(HexagonalCollider, decided_position, Quaternion.identity);
                    collider.transform.localScale = new Vector3(collider.transform.localScale.x, collider.transform.localScale.y * (tile_positions[i] + 1), collider.transform.localScale.z);
                    collider.transform.parent = ColliderHolder;
                }

                // Track meshes and their types
                switch(type){
                    case "WATER":
                        if(!AnimatedWater)
                            water_mesh.Add(tile.transform.GetChild(0).GetComponent<MeshFilter>());
                        else{
                            water_transforms[i] = tile.transform;
                        }
                        break;
                    case "GRASS":
                        grass_mesh.Add(tile.transform.GetChild(0).GetComponent<MeshFilter>());
                        break;
                    case "SAND":
                        sand_mesh.Add(tile.transform.GetChild(0).GetComponent<MeshFilter>());
                        break;
                    case "STONE":
                        stone_mesh.Add(tile.transform.GetChild(0).GetComponent<MeshFilter>());
                        break;
                }
            }

            GeneratePieceModel(i);

            // Counters
            x++;
            if(x >= MapSize){
                x = 0;
                y++;
            }
            if(ybounce == 0){
                ybounce = 0.5f;
            }
            else{
                ybounce = 0f;
            }
        }

        CombineMapMeshes();

        st.Stop();
        Debug.Log(string.Format("New map generated in {0} ms", st.ElapsedMilliseconds));

        ready = true;
    }

    void SetupPlayerBorders(){
        for(int player=0; player < _SessionManager.GetPlayerCount(); player++){
            GameObject g = GameObject.Instantiate(BorderHolder.gameObject);
            g.transform.parent = BorderHolder;
            player_border_holders.Add(g.transform);

            RefreshBorderMesh(player);
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
        water_transforms = new Transform[map_data_raw.Length];
        tiles_visible = new bool[map_data_raw.Length];
        tiles_created = new bool[map_data_raw.Length];
        piece_transforms = new Transform[map_data_raw.Length];
        tile_positions = new float[map_data_raw.Length];
        tile_data = new int[map_data_raw.Length];
        requires_piece_refresh = new bool[map_data_raw.Length];
        
        for(int i = 0; i < tile_data.Length; i++){
            tile_data[i] = _TileLookup.ID("Unmarked");
            requires_piece_refresh[i] = true;
            tile_positions[i] = map_data_raw[i] * TileHeightVariation;
        }

        VisibleTilesPass();
        BaseTilePass();
        GenerateMap();
    }

    public void VisibleTilesPass(){
        for(int i = 0; i < tiles_owned.Length; i++){
            if(tiles_owned[i] == _FactionLookup.ID(_SessionManager.OurInstance.FactionData())){
                MarkTileAsVisible(i);
            }

            if(tile_pieces[i] == _PieceLookup.ID(_SessionManager.OurInstance.FactionData().Tower())){
                _SessionManager.OurInstance.SnapCameraToPosition(GetTilePosition(i) + new Vector3(8,0,0));
            }
        }
    }

    public void ResetMapData(){
        water_transforms = new Transform[map_data_raw.Length];
        tile_pieces = new int[map_data_raw.Length];
        tiles_owned = new int[map_data_raw.Length];
        tiles_visible = new bool[map_data_raw.Length];
        tiles_created = new bool[map_data_raw.Length];
        piece_transforms = new Transform[map_data_raw.Length];
        tile_positions = new float[map_data_raw.Length];
        tile_data = new int[map_data_raw.Length];
        requires_piece_refresh = new bool[map_data_raw.Length];

        for(int i = 0; i < tiles_owned.Length; i++){
            tiles_owned[i] = -1;
        }
        
        for(int i = 0; i < tile_data.Length; i++){
            tile_data[i] = _TileLookup.ID("Unmarked");
        }
    }

    GameObject GetTilePrefab(int i){
        return _TileLookup.Tile(tile_data[i]).Prefab();
    }

    void MarkTileType(int i){
        if(map_data_raw[i] > StoneLimit)
            tile_data[i] = _TileLookup.ID("Stone");
        else if(map_data_raw[i] > GrassLimit)
            tile_data[i] = _TileLookup.ID("Grass");
        else
            tile_data[i] = _TileLookup.ID("Water");
    }

    void MarkSandTiles(int i){
        if(tile_data[i] == _TileLookup.ID("Water")){
            foreach(int p in GetNeighbors(i)){
                TileToSand(p);
            }
        }
    }

    void TileToSand(int pos){
        if(ValidateTile(pos)){
            if(tile_data[pos] == _TileLookup.ID("Grass")){
                tile_data[pos] = _TileLookup.ID("Sand");
            }
        }
    }
    
    void PlaceTowers(int castles_needed){
        // Use of a minimum distance that gets lower as the failed attempts increase
        // Ensure that the castles are placed randomly but far enough apart that the game is fair
        // But if no such position can be found quickly, the distance needed is reduced until it can be

        List<Vector2Int> placed_castles = new List<Vector2Int>();
        int minimum_distance = MapSize / castles_needed;
        minimum_distance = minimum_distance * minimum_distance;
        int distance_fails = 0;

        while(placed_castles.Count < castles_needed){

            if(distance_fails > 10)
                minimum_distance -= 1;

            int local = Random.Range(0, MapSize * MapSize);
            Vector2Int our_coords = TileToCoords(local);

            if(our_coords.x > 2 && our_coords.x < MapSize - 2 && our_coords.y > 2 && our_coords.y < MapSize - 2){
                if(tile_pieces[local] == 0){ // Empty
                    if(tile_data[local] == _TileLookup.ID("Grass") || tile_data[local] == _TileLookup.ID("Sand")){ // Basic ground
                        
                        bool valid = true;

                        foreach(Vector2Int pos in placed_castles){
                            if((our_coords - pos).sqrMagnitude < minimum_distance){
                                valid = false;
                            }
                        }

                        if((valid)){
                            PlayerInstance player = _SessionManager.GetPlayer(placed_castles.Count);
                            int _owner = _FactionLookup.ID(player.FactionData());

                            placed_castles.Add(TileToCoords(local));
                            PlacePiece(local, _PieceLookup.ID(player.FactionData().Tower()));

                            MarkRadiusAsOwned(local, 3, _owner);
                        }
                        else{
                            distance_fails++;
                        }
                    }
                }
            }
        }
    }

    Vector2Int TileToCoords(int id){
        int y = id / MapSize;
        int x = id - (MapSize * y);
        return (new Vector2Int(x, y));
    }

    // TILE OWNERSHIP //

    public bool CheckTileOwnership(int id, int owner){
        return (tiles_owned[id] == owner);
    }

    void MarkRadiusAsOwned(int id, int radius, int owner){
        MarkTileAsOwned(id, owner);
        foreach(int tile in TilesByDistance(id, radius, false)){
            MarkTileAsOwned(tile, owner);
        }
    }

    void MarkTileAsOwned(int id, int owner){
        if(ValidateTile(id)){
            if(tiles_owned[id] == -1)
                tiles_owned[id] = owner;
        }
    }

    bool map_regen_marker = false;
    public void MarkRadiusAsVisible(int id, int radius){
        MarkTileAsVisible(id);
        foreach(int tile in TilesByDistance(id, radius, false)){
            MarkTileAsVisible(tile);
        }
    }

    public void MarkTileAsVisible(int id){
        bool flag = (tiles_visible[id] == false);
        tiles_visible[id] = true;
        
        if(flag){
            map_regen_marker = true;
        }
    }

    public void CheckForMapRegen(){
        if(map_regen_marker){
            map_regen_marker = false;
            GenerateVisibleMapMesh();
            RefreshAllBorders();
        }
    }

    void RefreshAllBorders(){
        for(int i = 0; i < _SessionManager.GetPlayerCount(); i++)
            RefreshBorderMesh(i);
    }

    void RefreshBorderMesh(int player){
        int owner = _FactionLookup.ID(_SessionManager.GetPlayer(player).FactionData());
        Transform border_holder = player_border_holders[player];

        // Generic so this can be later expanded for different owners with different border meshes
        foreach(Transform t in border_holder)
            Destroy(t.gameObject);

        List<MeshFilter> border_pieces = new List<MeshFilter>();

        for(int tile_id = 0; tile_id < tiles_owned.Length; tile_id++){
            if(tiles_owned[tile_id] == owner)
                border_pieces.AddRange(PlaceNewBorders(tile_id, owner, border_holder));
        }

        Material mat = new Material(BorderMaterial);
        mat.SetColor("_BaseColour", _SessionManager.GetPlayer(player).FactionData().BorderColour());

        CombineMeshes(ref border_pieces, mat, border_holder);
    }

    List<MeshFilter> border_buffer = new List<MeshFilter>();
    List<MeshFilter> PlaceNewBorders(int tile, int owner, Transform bh){
        
        border_buffer = new List<MeshFilter>();
        
        if(!tiles_visible[tile] || !tiles_created[tile])
            return border_buffer;

        CreateBorder(tile, tile + MapSize, bh, owner, 1);
        CreateBorder(tile, tile - MapSize, bh, owner, 4);
        if(tile % 2 == 0){ // Down Tile
            CreateBorder(tile, tile + 1, bh, owner, 2); //TR
            CreateBorder(tile, tile - 1, bh, owner, 6); //TL
            CreateBorder(tile, tile - MapSize - 1, bh, owner, 5); // BL
            CreateBorder(tile, tile - MapSize + 1, bh, owner, 3); // BR
        }
        else{ // Up Tile
            CreateBorder(tile, tile + 1, bh, owner, 3); //BR
            CreateBorder(tile, tile - 1, bh, owner, 5); //BL
            CreateBorder(tile, tile + MapSize - 1, bh, owner, 6); // TL
            CreateBorder(tile, tile + MapSize + 1, bh, owner, 2); // TR
        }

        return border_buffer;
    }

    void CreateBorder(int tile, int comp_tile, Transform border_holder, int owner, int prefab){
        prefab = prefab - 1;

        bool border_here = false;
        if(ValidateTile(comp_tile)){
            if(tiles_owned[comp_tile] != owner){
                border_here = true;
            }
        }
        else{
            border_here = true;
        }

        if(border_here && tiles_created[tile]){
            Vector3 pos = GetTilePosition(tile);
            pos = new Vector3(pos.x, 0, pos.z);
            GameObject border_obj = GameObject.Instantiate(BorderPrefabs[prefab], pos, Quaternion.identity);
            border_obj.transform.parent = border_holder;
            border_buffer.Add(border_obj.transform.GetChild(0).GetComponent<MeshFilter>());
        }
    }

    // PIECE PLACING //

    void CheckTileExtras(int i){
        if(tile_pieces[i] != 0)
            return;

        // Grass fill
        if(tile_data[i] == _TileLookup.ID("Grass")){
            if(Random.Range(0.2f, 1f) + map_data_raw[i] >= 1){
                CoinFlipPiece(i, "Tree Large", "Tree Small");
            }

            RandomChancePiece(i, 60, "Piggie");
            RandomChancePiece(i, 60, "Piggie (Grass)");
            RandomChancePiece(i, 8, "Tall Grass");
            RandomChancePiece(i, 28, "Farm");
        }

        // Sand fill
        if(tile_data[i] == _TileLookup.ID("Sand")){
            RandomChancePiece(i, 40, "Palm Tree");
            RandomChancePiece(i, 40, "X Mark");
        }
        
        // Stone fill
        if(tile_data[i] == _TileLookup.ID("Stone")){
            if(Random.Range(0f, 0.5f) + map_data_raw[i] >= 1){
                PlacePiece(i, _PieceLookup.ID("Mountain"));
            }
        }

        // Ocean fill
        if(tile_data[i] == _TileLookup.ID("Water")){
            RandomChancePiece(i, 50, "Sharkfin");
        }
    }

    void RandomChancePiece(int i, int odds, string piece){
        if(tile_pieces[i] != 0)
            return;

        if(Random.Range(0, odds + 1) == 0)
            PlacePiece(i, _PieceLookup.ID(piece));
    }

    void CoinFlipPiece(int i, string piece_a, string piece_b){
        if(Random.Range(0, 3) == 0)
            PlacePiece(i, _PieceLookup.ID(piece_a));
        else
            PlacePiece(i, _PieceLookup.ID(piece_b));
    }

    void PlacePiece(int pos, int id){
        tile_pieces[pos] = id;
        requires_piece_refresh[pos] = true;
    }

    void GeneratePieceModel(int pos){

        if(tile_pieces[pos] <= 0 || requires_piece_refresh[pos] == false || !tiles_created[pos] || !tiles_visible[pos])
            return;

        requires_piece_refresh[pos] = false;
        if(piece_transforms[pos] != null){
            GameObject.Destroy(piece_transforms[pos].gameObject);
        }

        GameObject g = GameObject.Instantiate(_PieceLookup.Piece(tile_pieces[pos]).Prefab(), GetTilePosition(pos), Quaternion.identity);
        if(_PieceLookup.Piece(tile_pieces[pos]).RandomRotation())
            g.transform.eulerAngles = new Vector3(0, Random.Range(0f, 360f), 0f);
        if(_PieceLookup.Piece(tile_pieces[pos]).RandomChildRotation())
            g.transform.GetChild(0).eulerAngles = new Vector3(0, Random.Range(0f, 360f), 0f);
        piece_transforms[pos] = g.transform;
        g.transform.parent = PieceHolder;
    }

    // NOISE GENERATION //

    public void EstablishNoiseMap(){
        _NoiseManager.ImageWidth = MapSize;
        _NoiseManager.ImageHeight = MapSize;
        _NoiseManager.NewCachedNoise();
        map_data_raw = _NoiseManager.GetCachedNoise();
        SetImage();
        MapHolder.localScale = new Vector3(MapSize, MapSize, MapSize);
        GrassLimit = Random.Range(-0.2f, -0.6f);
    }

    void SetImage(){
        PreviewRenderer.material.mainTexture = _NoiseManager.NoiseAsImage(_NoiseManager.GetCachedNoise(), _NoiseManager.ImageWidth, _NoiseManager.ImageHeight);
    }

    // LARGE DATA SETTING //

    public void SetMapDataRaw(float[] data){
        map_data_raw = data;
    }

    public void SetGrassLimit(float limit){
        GrassLimit = limit;
    }

    public void SetTileOwnership(int[] data){
        tiles_owned = data;
    }

    public void SetTilePieces(int[] data){
        tile_pieces = data;
    }

    // GETTERS //

    public bool CheckVisibility(int tile){return tiles_visible[tile];}
}
