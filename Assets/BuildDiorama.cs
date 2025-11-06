using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildDiorama : MonoBehaviour
{
    [SerializeField] Transform[] Bricks;
    float[] target_y;

    const float minimal = -4f;
    const float offset = -1f;
    const float speed = 5f;

    void Start(){
        target_y = new float[Bricks.Length];
        for(int i = 0; i < target_y.Length; i++)
            target_y[i] = 0.1f;
        for(int i = 0; i < Bricks.Length; i++)
            Bricks[i].transform.position = new Vector3(Bricks[i].transform.position.x, minimal + (offset * i), Bricks[i].transform.position.z);
    }

    void Update(){
        for(int i = 0; i < Bricks.Length; i++){
            float new_y = Mathf.Lerp(Bricks[i].transform.position.y, target_y[i], Time.deltaTime * speed);
            Bricks[i].transform.position = new Vector3(Bricks[i].transform.position.x, new_y, Bricks[i].transform.position.z);
            if(new_y > target_y[i] - 0.05f)
                target_y[i] = 0;
        }
    }
}
