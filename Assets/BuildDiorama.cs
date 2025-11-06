using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenericUtils;

public class BuildDiorama : MonoBehaviour
{
    [SerializeField] Transform[] Bricks;
    [SerializeField] SoundEffect SwooshUp;
    [SerializeField] SoundEffect Click;

    float[] target_y;
    bool[] sfx_checked;
    int click_counter;

    const float minimal = -4f;
    const float offset = -1f;
    const float speed = 5f;
    const int nth_click = 4;

    void Start(){
        PlaySFX(SwooshUp);
        Setup();
    }

    void Setup(){
        target_y = new float[Bricks.Length];
        sfx_checked = new bool[Bricks.Length];
        for(int i = 0; i < target_y.Length; i++){
            target_y[i] = 0.1f;
            Bricks[i].transform.localPosition = new Vector3(Bricks[i].transform.localPosition.x, minimal + (offset * i), Bricks[i].transform.localPosition.z);
        }
    }

    void Update(){
        for(int i = 0; i < Bricks.Length; i++){
            float new_y = Mathf.Lerp(Bricks[i].transform.localPosition.y, target_y[i], Time.deltaTime * speed);
            Bricks[i].transform.localPosition = new Vector3(Bricks[i].transform.localPosition.x, new_y, Bricks[i].transform.localPosition.z);
            if(new_y > target_y[i] - 0.05f)
                target_y[i] = 0;
            if(target_y[i] == 0 && new_y < 0.02f && !sfx_checked[i]){
                sfx_checked[i] = true;
                if(CanClick())
                    PlaySFX(Click);
                IterateClick();
            }
        }
    }

    bool CanClick(){
        return click_counter == 0;
    }

    void IterateClick(){
        click_counter++;
        if(click_counter >= nth_click)
            click_counter = 0;
    }
}
