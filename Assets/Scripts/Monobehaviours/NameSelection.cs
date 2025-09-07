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
    [SerializeField] TextAsset Titles;
    [SerializeField] TextAsset Names;
    [SerializeField] SoundEffectLookup SFX_Lookup;
    [Header("UI")]
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
        DecompressTextFiles();
        our_title = ChooseRandomString(ref titles);
        our_name = ChooseRandomString(ref names);
        UpdateUI();
    }

    // Start Only //

    void DecompressTextFiles(){
        titles = Titles.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        names = Names.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
    }

    // Runtime //

    string ChooseRandomString(ref string[] options){
        return options[Random.Range(0, options.Length)];
    }

    void UpdateUI(){
        TitleDisplay.text = our_title;
        NameDisplay.text = our_name;
    }

    // UI Interactions //

    public void RandomiseTitle(){
        PlaySFX("UI_1", SFX_Lookup);
        our_title = ChooseRandomString(ref titles);
        UpdateUI();
    }

    public void RandomiseName(){
        PlaySFX("UI_1", SFX_Lookup);
        our_name = ChooseRandomString(ref names);
        UpdateUI();
    }

    public void ConfirmName(){
        PlaySFX("UI_2", SFX_Lookup);
        PlayerPrefs.SetString("USERNAME", our_title + " " + our_name);
        Menu.SetActive(false);
        Model.SetActive(false);
        SceneManager.LoadScene(NextScene);
    }
}
