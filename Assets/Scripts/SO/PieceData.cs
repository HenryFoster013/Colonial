using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Piece", menuName = "Custom/Pieces/Piece")]
public class PieceData : ScriptableObject
{
    [Header("Main")]
    [SerializeField] string _Type;
    [SerializeField] string _Name;  
    [Header("Display")]  
    [SerializeField] GameObject _Prefab;
    [SerializeField] float _TroopOffset;
    [SerializeField] bool _RandomiseRotation;
    [SerializeField] bool _RandomiseFirstChildRotation;
    [SerializeField] bool _ContainsBillboards;
    [SerializeField] Vector3 _RandomiseScale;
    [Header("Gameplay")]
    [SerializeField] bool _Walkable;
    [SerializeField] bool _Fort;
    [SerializeField] Faction _OwningFaction;
    [SerializeField] int _Population;
    [SerializeField] int _Produce;
    [SerializeField] int _Industry;
    [Header("Placement")]
    [SerializeField] int _Cost;
    [SerializeField] TileData[] _CompatibleTiles;
    [SerializeField] PieceData[] _CompatiblePieces;
    [SerializeField] bool _PlayConstructionSound;
    

    public bool CheckType(string s){
        return (s.ToUpper() == _Type.ToUpper());
    }

    public bool CheckName(string s){
        return (s.ToUpper() == _Name.ToUpper());
    }

    public bool Compatible(TileData tile){
        bool found = false;
        for(int i = 0; i < _CompatibleTiles.Length && !found; i++)
            found = _CompatibleTiles[i] == tile;
        return found;
    }

    public bool Compatible(PieceData piece){
        bool found = false;
        for(int i = 0; i < _CompatiblePieces.Length && !found; i++){
            found = _CompatiblePieces[i] == piece;
        }
        return found;
    }

    public bool Walkable(){return _Walkable;}
    public int Cost(){return _Cost;}
    public int Population(){return _Population;}
    public int Produce(){return _Produce;}
    public int Industry(){return _Industry;}
    public bool ContainsBillboards(){return _ContainsBillboards;}
    public GameObject Prefab(){return _Prefab;}
    public string Name(){return _Name;}
    public string Type(){return _Type;}
    public bool RandomRotation(){return _RandomiseRotation;}
    public bool RandomChildRotation(){return _RandomiseFirstChildRotation;}
    public float TroopOffset(){return _TroopOffset;}
    public bool Fort(){return _Fort;}
    public Faction Owner(){return _OwningFaction;}
    public bool PlayConstructionSound(){return _PlayConstructionSound;}
    public Vector3 RandomiseScale(){return _RandomiseScale;}
}



