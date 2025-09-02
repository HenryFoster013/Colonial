using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Lookup", menuName = "Custom/Tiles/Lookup")]
public class TileLookup : ScriptableObject
{
    [SerializeField] TileData[] Tiles;

    public TileData Tile(int i){
        return Tiles[i];
    }   

    public int ID(string name){
        int return_val = 0;
        bool done = false;
        for(int i = 0; i < Tiles.Length && !done; i++){
            if(Tiles[i].CheckType(name)){
                done = true;
                return_val = i;
            }
        }
        return return_val;
    }
}
