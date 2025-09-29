//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

namespace HenrysMapUtils{
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
        
        public int bonus_data {get; private set;}

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
            bonus_data = 0;
        }

        // Setters //

        public void Visible(){visible = true;}
        public void Created(){created = true;}
        public void SetOwner(Faction _owner){owner = _owner;}
        public void SetPiece(PieceData _piece){piece = _piece;}
        public void SetWaterTransform(Transform _transform){water_transform = _transform;}
        public void SetBonusData(int bonus){bonus_data = bonus;}
        public void SetPieceTransform(Transform _piece_transform){piece_transform = _piece_transform;}
        public void SetType(TileData new_type){type = new_type;}
        public void SetPosition(Vector3 position_){world_position = position_;}
        public void SetStats(TileStats _stats){stats = _stats;}
    }

    public class TileStats{

        public Tile tile {get; private set;}
        public string name {get; private set;}
        public int ownership_radius {get; private set;}
        public int money_produced {get; private set;}

        public int max_population {get; private set;}
        public int population_used {get; private set;}
        public int max_produce {get; private set;}
        public int produce_used {get; private set;}
        public int max_industry {get; private set;}
        public int industry_used {get; private set;}

        public TileStats(Tile _tile, string _name, int radius, int money, int pop, int prod, int industry){
            tile = _tile;
            name = _name;
            ownership_radius = radius;
            money_produced = money;
            max_population = pop;
            max_produce = prod;
            max_industry = industry;
            population_used = 0;
            produce_used = 0;
            industry_used = 0;
        }

        // Setters //

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
    }
}