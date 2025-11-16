using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapUtils{

    public class Seed{

        public int value {get; private set;}
        System.Random random;

        public Seed(){
            value = Random.Range(0, 999999999);
            random = new System.Random(value);
        }

        public Seed(int seed){
            value = seed;
            random = new System.Random(value);
        }

        // By defalt returns [0,1], this multiplies that value by the range (max - min), then adds the offset (min)
        public float Range(float min, float max){return (float)(random.NextDouble() * (max - min) + min);}
        public int RangeInt(int min, int max){return random.Next(min, max);}
        public int RandomInt(){return random.Next();}
    }

    public class NoiseGenerator{
        
        Seed seed;
        int width;
        int height;
        float scale;
        int layers;
        float offset_range;

        public NoiseGenerator(Seed _seed, int _size, float _scale, int _layers, float _offset){
            seed = _seed;
            width = _size;
            height = _size;
            scale = _scale;
            layers = _layers;
            offset_range = _offset;
        }

        public float[] Generate(){

            float[] random_vals = RandomiseOffsets();
            float[] pixels = new float[width * height];

            for(int count = 1; count <= layers; count++){
                float amplitude = 1f / (float)count;
                
                for (int y = 0; y < height; y++){
                    for (int x = 0; x < width; x++){
                        float xCoord = random_vals[(count * 2) - 2] + (float)x / width * scale * count;
                        float yCoord = random_vals[(count * 2) - 1] + (float)y / height * scale * count;
                        float sample = Mathf.PerlinNoise(xCoord, yCoord);
                        pixels[y * width + x] += Mathf.Clamp((sample * 2f) - 1f, -1f, 1f) * amplitude;
                    }
                }
            }

            return pixels;
        }

        float[] RandomiseOffsets(){
            float[] random_vals = new float[layers * 2];
            for(int i = 0; i < layers * 2; i++){
                random_vals[i] = seed.Range(-offset_range, offset_range);
            }
            return random_vals;
        }
    }

    public class Tile{

        public int ID {get; private set;}
        public bool visible {get; private set;}
        public bool created {get; private set;}

        public TileData type {get; private set;}
        public PieceData piece {get; private set;}
        public Faction owner {get; private set;}

        public Vector3 world_position {get; private set;}
        public Vector3 origin_position {get; private set;}
        public Transform water_transform {get; private set;}
        public Transform piece_transform {get; private set;}

        public TileStats stats {get; private set;}
        public float raw {get; private set;}

        public Tile(int id, float _raw, TileData _type, PieceData _piece, Vector3 _position, Faction _owner){
            ID = id;
            raw = _raw;
            visible = false;
            created = false;
            type = _type;
            piece = _piece;
            owner = _owner;
            origin_position = _position;
            world_position = _position;
            water_transform = null;
        }

        // Setters //

        public void Visible(){visible = true;}
        public void Created(){created = true;}
        public void SetOwner(Faction _owner){owner = _owner;}
        public void SetPiece(PieceData _piece){piece = _piece;}
        public void SetWaterTransform(Transform _transform){water_transform = _transform;}
        public void SetPieceTransform(Transform _piece_transform){piece_transform = _piece_transform;}
        public void SetType(TileData new_type){type = new_type;}
        public void SetPosition(Vector3 position_){world_position = position_;}
        public void SetStats(TileStats _stats){stats = _stats;}

        // Getters //

        public bool EmptyPiece(){return piece == null || piece.CheckType("Unmarked");}
    }

    public class TileStats{

        public Tile tile {get; private set;}
        public string name {get; private set;}
        public int ownership_radius {get; private set;}
        public int level {get; private set;}

        MapManager _MapManager;
        List<Tile> owned_tiles = new List<Tile>();

        private int max_population;
        private int population_used;
        private int max_produce;
        private int produce_used;
        private int max_industry;
        private int industry_used ;

        const int upgrade_base = 20;
        const int upgrade_additive = 10;
        const int resources_per_level = 1;
        const int level_limit = 3;

        public TileStats(Tile _tile, string _name, int money, MapManager manager){
            _MapManager = manager;
            tile = _tile;
            tile.SetStats(this);

            SetOwnershipRadius(2);
            name = _name;

            ResetMaxes();
            ReleaseUsage();
        }

        void ResetMaxes(){
            max_population = 0;
            max_produce = 0;
            max_industry = 0;
        }

        public void ReleaseUsage(){
            produce_used = 0;
            industry_used = 0;
            population_used = 0;
        }

        public void RefreshDetails(MapManager Map){
            ResetMaxes();
            foreach(Tile searched_tile in Map.TilesByDistance(tile, ownership_radius, false, false)){
                max_population += searched_tile.piece.Population();
                max_produce += searched_tile.piece.Produce();
                max_industry += searched_tile.piece.Industry();
            }
        }

        public void AddTroopStats(TroopData troop){
            produce_used += troop.ProduceCost();
            population_used += troop.PopulationCost();
            industry_used += troop.IndustryCost();
        }

        public bool BelowLevelLimit(){
            return level < level_limit;
        }

        public bool CityCotainsPiece(PieceData piece){
            if(owned_tiles.Count == 0)
                return false;
            foreach(Tile t in owned_tiles){
                if(t.piece == piece)
                    return true;
            }
            return false;
        }

        // Setters //

        public void SetOwnershipRadius(int rad){
            ownership_radius = rad;
            owned_tiles = _MapManager.TilesByDistance(tile, ownership_radius, false, false);
        }

        public void SetName(string _name){name = _name;}

        public void SetLevel(int lvl){
            if(lvl > level_limit)
                return;
            
            level = lvl;
            SetOwnershipRadius(2 + level);
        }

        public void AddPopulation(int amount){population_used += amount;}
        public void SetPopulation(int amount){population_used = amount;}
        public void AddProduce(int amount){produce_used += amount;}
        public void SetProduce(int amount){produce_used = amount;}
        public void AddIndustry(int amount){industry_used += amount;}
        public void SetIndustry(int amount){industry_used = amount;}

        public void AddMaxPopulation(int amount){max_population += amount;}
        public void SetMaxPopulation(int amount){max_population = amount;}
        public void AddMaxProduce(int amount){max_produce += amount;}
        public void SetMaxProduce(int amount){max_produce = amount;}
        public void AddMaxIndustry(int amount){max_industry += amount;}
        public void SetMaxIndustry(int amount){max_industry = amount;}

        // Getters //

        public int FreePopulation(){return MaxPopulation() - population_used;}
        public int FreeProduce(){return MaxProduce() - produce_used;}
        public int FreeIndustry(){return MaxIndustry() - industry_used;}

        public int MaxPopulation(){return max_population + (level * resources_per_level);}
        public int MaxProduce(){return max_produce + (level * resources_per_level);}
        public int MaxIndustry(){return max_industry + (level * resources_per_level);}

        public int PopulationUsed(){return population_used;}
        public int ProduceUsed(){return produce_used;}
        public int IndustryUsed(){return industry_used;}

        public int Value(){return MaxIndustry() + MaxPopulation() + MaxProduce();}

        public int UpgradeCost(){return (upgrade_base + (upgrade_additive * level));}
    }
}