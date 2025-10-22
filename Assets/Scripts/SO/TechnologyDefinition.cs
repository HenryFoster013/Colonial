using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Technology Definition", menuName = "Custom/Tech Tree/Technology Definition")]
public class TechnologyDefinition : ScriptableObject{

    // These are the 'nodes' in the tree,
    // they point to other technologies
    // and specify costs and unlocks

    [Header(" - Tree - ")]
    [SerializeField] TechnologyDefinition[] FollowingTech;
    [Header(" - Information - ")]
    [SerializeField] string _Name;
    [SerializeField] string _Description;
    [SerializeField] Sprite _Image;
    [SerializeField] int _Cost;
    [Header(" - Unlocks - ")]
    [SerializeField] TroopData[] TroopUnlocks;
    [SerializeField] PieceData[] BuildingUnlocks;

    // FUNCIONALITY //

    // GETTERS //

    public TechnologyDefinition[] NextTech(){return FollowingTech;}
    public int Cost(){return _Cost;}
    public string Name(){return _Name;}
    public string Description(){return _Description;}
    public Sprite Graphic(){return _Image;}
    public TroopData[] Troops(){return TroopUnlocks;}
    public PieceData[] Buildings(){return BuildingUnlocks;}

}