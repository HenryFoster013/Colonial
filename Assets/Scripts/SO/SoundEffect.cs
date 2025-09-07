using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New SFX", menuName = "Custom/Sound/SFX")]
public class SoundEffect : ScriptableObject
{
    public string Name;
    [SerializeField] AudioClip BaseClip;
    public AudioMixerGroup Mixer;
     
    [SerializeField] AudioClip[] ClipRange;
    [SerializeField] float BasePitch = 1;
    [SerializeField] float PitchVariation = 0;
    [SerializeField] float BaseVolume = 1;
    [SerializeField] float VolumeVariation = 0;

    public float Pitch(){return RandomiseValue(BasePitch, PitchVariation);}
    public float Volume(){return RandomiseValue(BaseVolume, VolumeVariation);}

    float RandomiseValue(float based, float variation){
        if(variation == 0)
            return based;
        
        bool negative = (Random.Range(0, 2) == 0);
        float mult = 1;
        if(negative)
            mult = -1;
        
        return based + (Random.Range(0, variation) * mult);
    }

    public AudioClip Clip(){
        if(BaseClip != null)
            return BaseClip;
        
        return ClipRange[Random.Range(0, ClipRange.Length)];
    }
}
