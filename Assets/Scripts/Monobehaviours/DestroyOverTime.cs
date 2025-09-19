using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOverTime : MonoBehaviour
{
    [SerializeField] float Delay;

    void Start(){
        StartCoroutine(LIGHTWEIGHTBABY());
    }

    IEnumerator LIGHTWEIGHTBABY(){
        yield return new WaitForSeconds(Delay);
        Destroy(this.gameObject);
    }
}
