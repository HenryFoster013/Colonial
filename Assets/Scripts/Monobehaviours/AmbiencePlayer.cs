using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HenrysUtils;

public class AmbiencePlayer : MonoBehaviour{
    
    [SerializeField] SoundEffect[] SpecialAmbiences;
    [SerializeField] AudioSource BaseTrack;

    const float min_wait = 15f;
    const float max_wait = 30f;
    float clock;
    
    float fade_time = 3f;
    float max_volume;
    bool vol_up = true;

    void Start(){
        SetWait();
        VolumeSetup();
    }

    void Update(){
        Clock();
        Volume();
    }

    void SetWait(){
        clock = Random.Range(min_wait, max_wait);
    }

    void VolumeSetup(){
        max_volume = BaseTrack.volume;
        fade_time = fade_time / max_volume;
        BaseTrack.volume = 0;
    }

    void Clock(){
        clock -= Time.deltaTime;
        if(clock < 0){
            PlaySFX(SpecialAmbiences[Random.Range(0, SpecialAmbiences.Length)]);
            SetWait();
        }
    }

    void Volume(){
        if(!vol_up)
            return;
        
        BaseTrack.volume += (Time.deltaTime / fade_time);
        if(BaseTrack.volume > max_volume){
            BaseTrack.volume = max_volume;
            vol_up = false;
        }
    }
}
