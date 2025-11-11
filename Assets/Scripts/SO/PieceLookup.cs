using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Piece Lookup", menuName = "Custom/Pieces/Lookup")]
public class PieceLookup : ScriptableObject
{
    [SerializeField] PieceData[] Pieces;
    [SerializeField] PieceData[] Buildings;

    public PieceData[] Buildable(){
        return Buildings;
    }

    public PieceData Piece(int i){
        return Pieces[i];
    }   

    public PieceData Piece(string type){
        return Pieces[ID(type)];
    }

    public PieceData Piece_ByName(string name){
        return Pieces[ID_ByName(name)];
    }

    public int ID(string type){
        int return_val = 0;
        bool done = false;
        for(int i = 0; i < Pieces.Length && !done; i++){
            if(Pieces[i].CheckType(type)){
                done = true;
                return_val = i;
            }
        }
        return return_val;
    }

    public int ID(PieceData reference){
        int return_val = 0;
        bool done = false;
        for(int i = 0; i < Pieces.Length && !done; i++){
            if(Pieces[i] == reference){
                done = true;
                return_val = i;
            }
        }
        return return_val;
    }

    public int ID_ByName(string name){
        int return_val = 0;
        bool done = false;
        for(int i = 0; i < Pieces.Length && !done; i++){
            if(Pieces[i].CheckName(name)){
                done = true;
                return_val = i;
            }
        }
        return return_val;
    }
}
