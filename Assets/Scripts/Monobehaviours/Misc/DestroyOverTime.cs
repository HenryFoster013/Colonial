using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    [SerializeField] bool AutoDelete = true;
    [SerializeField] float Delay = 999;

    void Start(){
        if(AutoDelete)
            StartDeletion(Delay);
    }

    public void StartDeletion(float _time){
        StartCoroutine(LIGHTWEIGHTBABY(_time));
    }

    IEnumerator LIGHTWEIGHTBABY(float _time){
        yield return new WaitForSeconds(_time);
        Destroy(this.gameObject);
    }
}
