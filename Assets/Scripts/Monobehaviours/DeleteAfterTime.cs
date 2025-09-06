using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteAfterTime : MonoBehaviour
{
    public void StartDeletion(float _time){
        StartCoroutine(DeleteThis(_time));
    }

    IEnumerator DeleteThis(float _time){
        yield return new WaitForSeconds(_time);
        Destroy(this.gameObject);
    }
}
