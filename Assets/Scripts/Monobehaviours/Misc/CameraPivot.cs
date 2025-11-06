using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPivot : MonoBehaviour
{
    [SerializeField] Vector2 Sensitivity;
    Vector2Int mouse_offset;

    void Start(){
        mouse_offset = new Vector2Int(Screen.width / 2, Screen.height / 2);
    }

    public void Update(){
        float y = (Input.mousePosition.x - mouse_offset.x) * -Sensitivity.x;
        float x = (Input.mousePosition.y - mouse_offset.y) * Sensitivity.y;
        transform.eulerAngles = new Vector3(x, y, 0);
    }
}
