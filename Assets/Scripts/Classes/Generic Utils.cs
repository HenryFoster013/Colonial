using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using MapUtils;

public static class GenericUtils{
    
    // SOUND EFFECTS //
    
    public static void PlaySFX(string name, SoundEffectLookup SoundLookup){SpawnSound(SoundLookup.GetSFX(name));}
    public static void PlaySFX(int id, SoundEffectLookup SoundLookup){SpawnSound(SoundLookup.GetSFX(id));}
    public static void PlaySFX(SoundEffect sound){SpawnSound(sound);}

    public static void SpawnSound(SoundEffect sound){
        if(sound == null)
            return;

        GameObject new_sfx = new GameObject(sound.Name);
        AudioSource asrc = new_sfx.AddComponent<AudioSource>();
        asrc.clip = sound.Clip();
        asrc.volume = sound.Volume();
        asrc.pitch = sound.Pitch();
        asrc.outputAudioMixerGroup  = sound.Mixer;
        asrc.Play();
        DestroyOverTime deletor = new_sfx.AddComponent<DestroyOverTime>();
        deletor.StartDeletion(asrc.clip.length + 1f);
        GameObject.DontDestroyOnLoad(new_sfx);
    }

    // SEED GEN //

    public static void SeedShuffle<T>(ref T[] array, int seed){
        System.Random random = new System.Random(seed);
        T temp;
        int random_pointer = 0;

        for(int i = array.Length - 1; i >= 0; i--){
            random_pointer = random.Next(i + 1);
            while(random_pointer >= array.Length)
                random_pointer -= array.Length;
            temp = array[random_pointer];
            array[random_pointer] = array[i];
            array[i] = temp;
        }
    }

    // LOOKUPS //

    public static FactionLookup GetFactionLookup(){return Resources.Load<FactionLookup>("_ Faction Lookup");}
    public static PieceLookup GetPieceLookup(){return Resources.Load<PieceLookup>("_ Piece Lookup");}
    public static SoundEffectLookup GetSFXLookup(){return Resources.Load<SoundEffectLookup>("_ SFX Lookup");}
    public static TileLookup GetTileLookup(){return Resources.Load<TileLookup>("_ Tile Lookup");}
    public static TroopLookup GetTroopLookup(){return Resources.Load<TroopLookup>("_ Troop Lookup");}

    // VALIDATION //

    public static bool BuildingValid(Tile tile, PieceData piece){
        return piece.Compatible(tile.piece) && piece.Compatible(tile.type);
    }

    public static bool ValidateTroop(Troop troop){
        if(troop == null)
            return false;
        return troop.spawned;
    }

    public static bool IsTileFortress(Tile tile){
        return (tile.piece.CheckType("Tower") || tile.piece.CheckType("Fort"));
    }

    public static bool CheckTileOwnership(Tile tile, Faction owner){
        return (tile.owner == owner);
    }

    // MISC //

    public static int CountBooleanArray(bool[] array){
        int count = 0;
        foreach(bool owned in array){
            if(owned)
                count++;
        }
        return count;
    }

    public static void SetLayer(GameObject obj, int _layer){
        obj.layer = _layer;
        foreach (Transform child in obj.transform){
            child.gameObject.layer = _layer;
            if (child.GetComponentInChildren<Transform>() != null)
                SetLayer(child.gameObject, _layer);
        }
    }
}
