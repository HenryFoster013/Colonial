using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Nameset", menuName = "Custom/Nameset")]
public class Nameset : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] TextAsset Prefixes_TXT;
    [SerializeField] TextAsset Suffixes_TXT;
    [SerializeField] bool Whitespace = false;
    
    string[] Prefixes;
    string[] Suffixes;
    

    public string GenerateName(){
        if(Whitespace)
            return Prefixes[Random.Range(0, Prefixes.Length)] + Suffixes[Random.Range(0, Suffixes.Length)];
        else
            return Prefixes[Random.Range(0, Prefixes.Length)] + " " + Suffixes[Random.Range(0, Suffixes.Length)];
    }

    // Need to generate the arrays from the txt and save them, make immediate setup refresh the txt arrays
    
}
