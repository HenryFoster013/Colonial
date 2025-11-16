using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BG_MusicNode : MonoBehaviour
{
    [SerializeField] AudioSource _AudioSource;
    [HideInInspector] public AudioClip Clip;

    float target_volume = 0;
    bool mark_for_death = false;

    void Start(){
        DontDestroyOnLoad(this.gameObject);
    }

    public void Play(AudioClip clip, bool loop){
        Clip = clip;
        if(clip == null)
            Destroy(this.gameObject);
        _AudioSource.clip = clip;
        _AudioSource.Play();
        _AudioSource.loop = loop;
        target_volume = _AudioSource.volume;
        _AudioSource.volume = 0;
    }

    void LateUpdate(){
        if(_AudioSource.clip == null)
            return;
        _AudioSource.volume = Mathf.Lerp(_AudioSource.volume, target_volume, Time.deltaTime * 1.5f);
        if(mark_for_death){
            if(_AudioSource.volume < 0.01f){
                Destroy(this.gameObject);
            }
        }
    }

    public void Terminate(){
        target_volume = 0;
        mark_for_death = true;
    }

    public bool Validate(AudioClip clip){return _AudioSource.clip == clip;}
    public bool Deathly(){return mark_for_death;}
}
