using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SFX", menuName = "Custom/Sound/SFX")]
public class SoundEffect : ScriptableObject
{
    public AudioClip Clip {get; private set;}
    public float BasePitch {get; private set;} = 1;
    public float PitchVariation {get; private set;} = 0;
    public float BaseVolume {get; private set;} = 1;
    public float VolumeVariation {get; private set;} = 0;

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
}
