using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GenericUtils;

public class GenericLoadScene : MonoBehaviour
{
    [SerializeField] SoundEffectLookup SFX_Lookup;
    [SerializeField] BackgroundColouring BG;

    public void LoadScene(string scene){
        if(SFX_Lookup != null){
            PlaySFX("UI_2", SFX_Lookup);
        }

        if(BG != null)
            BG.Save();

        SceneManager.LoadScene(scene);
    }
}
