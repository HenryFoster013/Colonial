using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayTroop : MonoBehaviour
{
    [SerializeField] MeshRenderer[] Meshes;
    public void DisplayInitialSetup(SessionManager sm, int faction_id){
        MaterialPropertyBlock[] skins = sm.GetTroopMaterials(faction_id);
        foreach(MeshRenderer renderer in Meshes){
            renderer.SetPropertyBlock(skins[0]);
        }
    }
}
