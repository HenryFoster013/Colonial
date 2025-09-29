using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Faction", menuName = "Custom/Factions/Faction")]
public class Faction : ScriptableObject
{
    [Header(" - Main - ")]
    [SerializeField] string _Type;
    [SerializeField] string _Name;
    [Header(" - References - ")]
    [SerializeField] TroopData[] _Troops;
    [SerializeField] PieceData _Tower;
    [SerializeField] PieceData _Fort;
    [Header(" - Display - ")]
    [SerializeField] Color _Colour = new Color(1f,1f,1f,1f);
    [SerializeField] Color _BorderColour = new Color(1f,1f,1f,1f);
    [SerializeField] int _TextureOffset;
    [SerializeField] Sprite _Flag;
    [SerializeField] Sprite _Mini_Flag;
    [SerializeField] string _Currency;
    

    public bool CheckType(string s){
        return (s.ToUpper() == _Type.ToUpper());
    }

    public string Type(){return _Type;}
    public string Name(){return _Name;}
    public Color Colour(){return _Colour;}
    public Color BorderColour(){return _BorderColour;}
    public Sprite Flag(){return _Flag;}
    public Sprite Mini_Flag(){return _Mini_Flag;}
    public TroopData[] Troops(){return _Troops;}
    public PieceData Tower(){return _Tower;}
    public PieceData Fort(){return _Fort;}
    public int TextureOffset(){return _TextureOffset;}
    public string Currency(){return _Currency;}
}
