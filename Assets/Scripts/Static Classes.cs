using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public static class HenrysUtils{
    
    // SOUND EFFECTS //
    
    public static void PlaySFX(string name, SoundEffectLookup SoundLookup){SpawnSound(SoundLookup.GetSFX(name));}
    public static void PlaySFX(int id, SoundEffectLookup SoundLookup){SpawnSound(SoundLookup.GetSFX(id));}
    public static void PlaySFX(SoundEffect sound){SpawnSound(sound);}

    public static void SpawnSound(SoundEffect sound){
        GameObject new_sfx = new GameObject(sound.Name);
        AudioSource asrc = new_sfx.AddComponent<AudioSource>();
        asrc.clip = sound.Clip();
        asrc.volume = sound.Volume();
        asrc.pitch = sound.Pitch();
        asrc.outputAudioMixerGroup  = sound.Mixer;
        asrc.Play();
        DeleteAfterTime deletor = new_sfx.AddComponent<DeleteAfterTime>();
        deletor.StartDeletion(asrc.clip.length + 1f);
        GameObject.DontDestroyOnLoad(new_sfx);
    }

    // LOOKUPS //

    public static FactionLookup GetFactionLookup(){return Resources.Load<FactionLookup>("_ Faction Lookup");}
    public static PieceLookup GetPieceLookup(){return Resources.Load<PieceLookup>("_ Piece Lookup");}
    public static SoundEffectLookup GetSFXLookup(){return Resources.Load<SoundEffectLookup>("_ SFX Lookup");}
    public static TileLookup GetTileLookup(){return Resources.Load<TileLookup>("_ Tile Lookup");}
    public static TroopLookup GetTroopLookup(){return Resources.Load<TroopLookup>("_ Troop Lookup");}
}
