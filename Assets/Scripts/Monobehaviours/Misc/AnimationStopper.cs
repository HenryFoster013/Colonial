using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStopper : MonoBehaviour
{
    [SerializeField] Animator Anim;
    [SerializeField] GameObject[] Wanted;
    [SerializeField] GameObject[] Unwanted;
    [SerializeField] GameObject[] AlwaysKill;

    public void Chill(bool standard){
        Destroy(Anim);

        if(standard){
            foreach(GameObject g in Unwanted)
                Destroy(g);
            foreach(GameObject g in Wanted)
                g.SetActive(true);
        }
        else{
            foreach(GameObject g in Wanted)
                Destroy(g);
            foreach(GameObject g in Unwanted)
                g.SetActive(true);           
        }
        foreach(GameObject g in AlwaysKill)
            Destroy(g);
    }
}
