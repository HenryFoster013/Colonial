using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Billboard : MonoBehaviour
{
    public Transform m_Camera;
    [SerializeField] bool FixY;
    bool buffered = false;

    void OnEnable(){
        StartCoroutine(BufferFrames());
    }

    void LateUpdate(){
        if(!buffered)
            return;
        
        if(m_Camera == null){
            m_Camera = GameObject.FindGameObjectWithTag("Render Point").transform;
        }
        else{
            if(FixY)
                transform.LookAt(new Vector3(m_Camera.position.x,transform.position.y,m_Camera.position.z));    
            else
                transform.LookAt(m_Camera.position);
        }
    }

    IEnumerator BufferFrames(){
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if(m_Camera == null)
            m_Camera = GameObject.FindGameObjectWithTag("Render Point").transform;
        buffered = true;
    }
}
