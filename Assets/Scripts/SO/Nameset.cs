using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HenrysUtils;

[CreateAssetMenu(fileName = "New Nameset", menuName = "Custom/Nameset")]
public class Nameset : ScriptableObject
{
    [Header("Main")]
    [SerializeField] TextAsset Prefixes_TXT;
    [SerializeField] TextAsset Suffixes_TXT;
    [SerializeField] bool Whitespace = false;
    
    public string[] Prefixes;
    public string[] Suffixes;

    public string RandomPrefix(){return Prefix(Random.Range(0, Prefixes.Length));}
    public string RandomSuffix(){return Suffix(Random.Range(0, Suffixes.Length));}
    public string Prefix(int prex){
        if(prex < 0 || prex >= Prefixes.Length)
            return("silly");
        return Prefixes[prex];
    }
    public string Suffix(int sufx){
        if(sufx < 0 || sufx >= Suffixes.Length)
            return("billy");
        return Suffixes[sufx];
    }

    public string RandomName(){
        Debug.Log(Prefixes);
        Debug.Log(Suffixes);
        return Name(Random.Range(0, Prefixes.Length), Random.Range(0, Suffixes.Length));
    }
    public string Name(int prex, int sufx){
        if(!Whitespace)
            return Prefix(prex) + Suffix(sufx);
        else
            return Prefix(prex) + " " + Suffix(sufx);
    }

    public void RegenerateArrays(){
        Prefixes = Prefixes_TXT.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        Suffixes = Suffixes_TXT.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
    }

    public void Shuffle(int pref_seed, int sufx_seed){
        SeedShuffle(ref Prefixes, pref_seed);
        SeedShuffle(ref Suffixes, sufx_seed);
    }    

    public string GetLocationName(int tile_id){
        int prefix_number = tile_id;
        int suffix_number = tile_id;

        while(prefix_number >= Prefixes.Length){
            prefix_number -= Prefixes.Length;
        }
        while(suffix_number >= Suffixes.Length){
            suffix_number -= Suffixes.Length;
        }

        return Name(prefix_number, suffix_number);
    }
}
