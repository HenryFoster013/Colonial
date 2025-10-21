using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Technology", menuName = "Custom/Tech Tree/Technology")]
public class Technology : ScriptableObject{

    // These are the 'nodes' in the tree,
    // they point to other technologies
    // and specify costs and unlocks

    [Header(" - Tree - ")]
    [SerializeField] Technology[] FollowingNodes;
    [Header(" - Information - ")]
    [SerializeField] string _Name;
    [SerializeField] Sprite _Image;
    [SerializeField] int _Cost;
    [Header(" - Unlocks - ")]
    [SerializeField] TroopData[] TroopUnlocks;
    [SerializeField] PieceData[] BuildingUnlocks;

    // FUNCIONALITY //

    // GETTERS //

    public Technology[] NextNodes(){return FollowingNodes;}
    public int Cost(){return _Cost;}
    public int Name(){return _Name;}
    public int Display(){return _Image;}
    public TroopData[] Troops(){return TroopUnlocks;}
    public PieceData[] Buildings(){return BuildingUnlocks;}

}