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
    }
}