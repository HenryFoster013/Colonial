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
    [Header("Costs")]
    [SerializeField] int _Cost;
    [SerializeField] int _PopulationCost;
    [SerializeField] int _ProduceCost;
    [SerializeField] int _IndustryCost;
    [Header("Gameplay")]
    [SerializeField] int _MoveDistance;  
    [SerializeField] int _AttackDistance;  
    [SerializeField] int _Vision;
    [SerializeField] int _Health;
    [SerializeField] int _Damage;
    [SerializeField] bool _MoveOnCloseKill;
    [Header("Prefabs")]  
    [SerializeField] GameObject _Prefab;
    [SerializeField] NetworkPrefabRef _NetPrefabRef;
    [Header("Animations")]
    [SerializeField] string _AttackAnim = "TroopAttack";

    public bool CheckType(string s){
        return (s.ToUpper() == _Type.ToUpper());
    }
    public int Cost(){return _Cost;}
    public int PopulationCost(){return _PopulationCost;}
    public int ProduceCost(){return _ProduceCost;}
    public int IndustryCost(){return _IndustryCost;}
    public int Vision(){return _Vision;}
    public GameObject Prefab(){return _Prefab;}
    public NetworkPrefabRef NetPrefabRef(){return _NetPrefabRef;}
    public string Name(){return _Name;}
    public string Type(){return _Type;}
    public int MoveDistance(){return _MoveDistance;}
    public int AttackDistance(){return _AttackDistance;}
    public int Damage(){return _Damage;}
    public int Health(){return _Health;}
    public string AttackAnim(){return _AttackAnim;}
    public bool MoveOnCloseKill(){return _MoveOnCloseKill;}
}
