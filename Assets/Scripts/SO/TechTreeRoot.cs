using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Root", menuName = "Custom/Tech Tree/Root")]
public class TechTreeRoot : ScriptableObject
{
    [Header(" - MAIN - ")]
    [SerializeField] TechnologyDefinition FirstTechnology;
    [Header(" - DISPLAY - ")]
    [SerializeField] string TreeTitle;
    [Header("Colours")]
    [SerializeField] Color BrightTone = new Color(1f,1f,1f,1f);
    [SerializeField] Color DarkTone = new Color(1f,1f,1f,1f);

    public string Title(){return TreeTitle;}
    public TechnologyDefinition FirstNode(){return FirstTechnology;}
    public Color BrightColour(){return BrightTone;}
    public Color DarkColour(){return DarkTone;}
}
