using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using static HenrysUtils;

public class NameSelection : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] string NextScene;
    [SerializeField] SoundEffectLookup SFX_Lookup;
    [SerializeField] Nameset PlayerNameset;
    [Header("UI")]
    [SerializeField] BackgroundColouring BG;
    [SerializeField] GameObject Menu;
    [SerializeField] GameObject Model;
    [SerializeField] TMP_Text TitleDisplay;
    [SerializeField] TMP_Text NameDisplay;

    string[] titles;
    string[] names;
    string our_title, our_name;
    
    void Start(){
        Menu.SetActive(true);
        Model.SetActive(true);
        our_title = PlayerNameset.RandomPrefix();
        our_name = PlayerNameset.RandomSuffix();
        UpdateUI();
    }

    // Runtime //

    void UpdateUI(){
        TitleDisplay.text = our_title;
        NameDisplay.text = our_name;
    }

    public void RandomiseTitle(){
        PlaySFX("UI_1", SFX_Lookup);
        our_title = PlayerNameset.RandomPrefix();
        UpdateUI();
    }

    public void RandomiseName(){
        PlaySFX("UI_1", SFX_Lookup);
        our_name = PlayerNameset.RandomSuffix();
        UpdateUI();
    }

    // UI Interactions //

    public void ConfirmName(){
        PlaySFX("UI_2", SFX_Lookup);
        PlayerPrefs.SetString("USERNAME", our_title + " " + our_name);
        Menu.SetActive(false);
        Model.SetActive(false);
        SceneManager.LoadScene(NextScene);
        BG.Save();
    }
}
