using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[CreateAssetMenu(fileName = "New Troop Type", menuName = "Custom/Troops/Type")]
public class TroopData : ScriptableObject
{
    [Header("Main")]
    [SerializeField] string _Type;
    [SerializeField] string _Name;  
    [SerializeField] int _Cost;
    [Header("Gameplay")]
    [SerializeField] int _MoveDistance;  
    [SerializeField] int _AttackDistance;  
    [SerializeField] int _Vision;
    [Header("Display")]  
    [SerializeField] GameObject _Prefab;
    [SerializeField] NetworkPrefabRef _NetPrefabRef;

    public bool CheckType(string s){
        return (s.ToUpper() == _Type.ToUpper());
    }
    public int Cost(){return _Cost;}
    public int Vision(){return _Vision;}
    public GameObject Prefab(){return _Prefab;}
    public NetworkPrefabRef NetPrefabRef(){return _NetPrefabRef;}
    public string Name(){return _Name;}
    public string Type(){return _Type;}
    public int MoveDistance(){return _MoveDistance;}
    public int AttackDistance(){return _AttackDistance;}
}
