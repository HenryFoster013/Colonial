using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sound Lookup", menuName = "Custom/Sound/Lookup")]
public class SoundEffectLookup : ScriptableObject
{
    public SoundEffect[] Sounds;

    public SoundEffect GetSFX(string name){
        SoundEffect result = null;
        for(int i = 0; i < Sounds.Length && result == null; i++){
            if(Sounds[i].Name.ToUpper() == name.ToUpper())
                result = Sounds[i];
        }
        return result;
    }

    public SoundEffect GetSFX(int id){
        return Sounds[id];
    }
}
