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
    [Header("Gameplay")]
    [SerializeField] bool _Walkable;
    [SerializeField] bool _Fort;

    public bool CheckType(string s){
        return (s.ToUpper() == _Type.ToUpper());
    }

    public bool CheckName(string s){
        return (s.ToUpper() == _Name.ToUpper());
    }

    public bool Walkable(){return _Walkable;}
    public bool ContainsBillboards(){return _ContainsBillboards;}
    public GameObject Prefab(){return _Prefab;}
    public string Name(){return _Name;}
    public string Type(){return _Type;}
    public bool RandomRotation(){return _RandomiseRotation;}
    public bool RandomChildRotation(){return _RandomiseFirstChildRotation;}
    public float TroopOffset(){return _TroopOffset;}
    public bool Fort(){return _Fort;}
}



