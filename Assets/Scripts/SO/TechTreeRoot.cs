using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Root", menuName = "Custom/Tech Tree/Root")]
public class TechTreeRoot : ScriptableObject
{
    [SerializeField] string TreeTitle;
    [SerializeField] TechnologyDefinition FirstTechnology;

    public string Title(){return TreeTitle;}
    public TechnologyDefinition FirstNode(){return FirstTechnology;}
}
