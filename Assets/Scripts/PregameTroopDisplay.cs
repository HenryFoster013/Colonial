using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PregameTroopDisplay : MonoBehaviour
{
    [SerializeField] GameObject[] Troops;


    void Start(){
        foreach(GameObject g in Troops)
            g.SetActive(false);
    }

    public void SetTroop(int id){
        for(int i = 0; i < Troops.Length; i++){
            Troops[i].SetActive(i == id);
        }
    }
}
