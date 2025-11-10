using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static GenericUtils;
using EventUtils;

public class SettingsWindow : Window{

    [Header("Main")]
    [SerializeField] SettingsManager Manager;
    [SerializeField] SoundEffect ResolutionSound;

    [Header("Volumes")]
    [SerializeField] Slider MainVolumeSlider;
    [SerializeField] Slider SFX_VolumeSlider;
    [SerializeField] Slider BG_VolumeSlider;

    [Header("Resolution")]
    [SerializeField] TMP_InputField ScreenWidth;
    [SerializeField] TMP_InputField ScreenHeight;
    [SerializeField] Toggle FullscreenCheck;

    int ui_sets;

    public override void Open(){
        if(dragRectTransform.gameObject.activeSelf){
            Close();
            return;
        }
        ui_sets = 0;
        PlaySFX(OpenSFX);
        LoadSettings();
        SilentOpen();
    }

    public void UpdateFullscreen(){
        bool fullscreen = FullscreenCheck.isOn;
        ScreenWidth.interactable = !fullscreen;
        ScreenHeight.interactable = !fullscreen;
    }

    void LoadSettings(){
        Manager.RefreshSettings();

        float main_vol = PlayerPrefs.GetFloat("VOLUME.MASTER");
        MainVolumeSlider.value = main_vol;

        float bg_vol = PlayerPrefs.GetFloat("VOLUME.BACKGROUND");
        BG_VolumeSlider.value = bg_vol;

        float sfx_vol = PlayerPrefs.GetFloat("VOLUME.SFX");
        SFX_VolumeSlider.value = sfx_vol;

        ScreenWidth.text = PlayerPrefs.GetInt("SCREEN.WIDTH").ToString();
        ScreenHeight.text = PlayerPrefs.GetInt("SCREEN.HEIGHT").ToString();

        FullscreenCheck.isOn = PlayerPrefs.GetInt("FULLSCREEN") == 1;
        UpdateFullscreen();
    }

    public void UpdateVolume(){
        ui_sets++;
        if(ui_sets < 4)
            return;

        PlayerPrefs.SetFloat("VOLUME.MASTER", MainVolumeSlider.value);
        PlayerPrefs.SetFloat("VOLUME.SFX", SFX_VolumeSlider.value);
        PlayerPrefs.SetFloat("VOLUME.BACKGROUND", BG_VolumeSlider.value);
        Manager.RefreshSettings();
    }

    public void UpdateResolution(){
        PlayerPrefs.SetInt("SCREEN.WIDTH", Resolutionify(ScreenWidth.text));
        PlayerPrefs.SetInt("SCREEN.HEIGHT", Resolutionify(ScreenHeight.text));
        int fullscreen = 0;
        if(FullscreenCheck.isOn)
            fullscreen = 1;
        PlayerPrefs.SetInt("FULLSCREEN", fullscreen);
        Manager.RefreshSettings();
        PlaySFX(ResolutionSound);
    }

    int Resolutionify(string val){
        if (int.TryParse(val, out int value)){
            if(value > 100 && value < 9999)
                return value;
            else
                return 600;
        }
        else
            return 600;
    }
}