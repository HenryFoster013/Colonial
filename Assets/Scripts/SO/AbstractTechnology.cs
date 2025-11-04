using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Abstract Technology", menuName = "Custom/Technology/Abstract")]
public class AbstractTechnology : ScriptableObject
{
    // Please note that this is NOT a technology definition.
    // These sit within technology definitions and are used to define abstract concepts as unlockable
    // These are not troops or pieces but rather other features such as offering peace, bargaining and messaging

    [Header("The name as checked by 'Unlock' functions.")]
    [SerializeField] string _Reference;
    [Header("'Unlocks ' (Description) ', ...'")]
    [SerializeField] string _Description;

    public string Reference(){return _Reference;}
    public string Description(){return _Description;}
}
