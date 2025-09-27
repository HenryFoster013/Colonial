using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Type", menuName = "Custom/Tiles/Type")]
public class TileData : ScriptableObject
{
    [Header("Main")]
    [SerializeField] string _Type;
    [Header("Display")]
    [SerializeField] string _Name;
    [SerializeField] Color _TabColour = new Color(1f,1f,1f,1f);
    [SerializeField] Color _TextColour = new Color(1f,1f,1f,1f);
    [Header("Gameplay")]
    [SerializeField] GameObject _Prefab;
    [SerializeField] bool _Walkable;

    public bool CheckType(string s){
        return (s.ToUpper() == Type());
    }

    public bool Walkable(){return _Walkable;}
    public GameObject Prefab(){return _Prefab;}
    public Color TabColour(){return _TabColour;}
    public Color TextColour(){return _TextColour;}
    public string Type(){return _Type.ToUpper();}
}
