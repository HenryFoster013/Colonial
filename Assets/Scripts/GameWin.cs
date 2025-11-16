using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static GenericUtils;
using UnityEngine.SceneManagement;
using TMPro;

public class GameWin : MonoBehaviour
{
    [SerializeField] SoundEffectLookup SFX_Lookup;
    [SerializeField] BackgroundColouring BG;
    [SerializeField] FactionLookup _FactionLookup;
    [SerializeField] TMP_Text Victory;
    [SerializeField] AudioMixer Mixer;
    [Header("Make sure these are in the same order as the Faction Lookup")]
    [SerializeField] GameObject[] TroopModels;
    [SerializeField] GameObject[] Flags;

    public void Continue(){
        PlaySFX("UI_2", SFX_Lookup);
        SceneManager.LoadScene("Title Screen");
        BG.Save();
    }

    void Start(){
        
        Mixer.SetFloat("MusicVolume", FloatToDecibel(1f));
        Mixer.SetFloat("AmbienceVolume", FloatToDecibel(0f));
        
        int winner = PlayerPrefs.GetInt("WINNER");

        foreach(GameObject g in TroopModels)
            g.SetActive(false);
        foreach(GameObject g in Flags)
            g.SetActive(false);
        TroopModels[winner].SetActive(true);
        Flags[winner].SetActive(true);

        Victory.text = _FactionLookup.GetFaction(winner).Victory();
    }
}
