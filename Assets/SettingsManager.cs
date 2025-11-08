using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static GenericUtils;

public class SettingsManager : MonoBehaviour{
    
    [SerializeField] AudioMixer Mixer;

    float volume_master, volume_sfx, volume_bg;
    int screen_width, screen_height;
    bool fullscreen;
    
    public void Setup(){
        DontDestroyOnLoad(this.gameObject);
        if(PlayerPrefs.GetInt("SETTINGS_SETUP") == 0)
            ResetSettings();
        RefreshSettings();
    }

    public void RefreshSettings(){
        LoadSettings();
        ApplySettings();
    }

    void ResetSettings(){
        PlayerPrefs.SetFloat("VOLUME.MASTER", 1f);
        PlayerPrefs.SetFloat("VOLUME.SFX", 1f);
        PlayerPrefs.SetFloat("VOLUME.BACKGROUND", 0.5f);
        PlayerPrefs.SetInt("FULLSCREEN", 0);
        PlayerPrefs.SetInt("SCREEN.WIDTH", 800);
        PlayerPrefs.SetInt("SCREEN.HEIGHT", 600);
    }

    void LoadSettings(){
        volume_master = Mathf.Clamp(PlayerPrefs.GetFloat("VOLUME.MASTER"), 0f, 1f);
        volume_sfx = Mathf.Clamp(PlayerPrefs.GetFloat("VOLUME.SFX"), 0f, 1f);
        volume_bg = Mathf.Clamp(PlayerPrefs.GetFloat("VOLUME.BACKGROUND"), 0f, 1f);
        screen_width = PlayerPrefs.GetInt("SCREEN.WIDTH");
        screen_height = PlayerPrefs.GetInt("SCREEN.HEIGHT");
        fullscreen = PlayerPrefs.GetInt("FULLSCREEN") == 1;
    }

    void ApplySettings(){
        Mixer.SetFloat("", FloatToDecibel(PlayerPrefs.GetFloat("VOLUME.MASTER")));
        Mixer.SetFloat("", FloatToDecibel(PlayerPrefs.GetFloat("VOLUME.SFX")));
        Mixer.SetFloat("", FloatToDecibel(PlayerPrefs.GetFloat("VOLUME.BACKGROUND")));
        
        if(PlayerPrefs.GetInt("FULLSCREEN") == 0)
            Screen.SetResolution(PlayerPrefs.GetInt("SCREEN.WIDTH"), PlayerPrefs.GetInt("SCREEN.HEIGHT"), false);
        else
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
    }

    public void SetVolume(string label, float amount){
        amount = Mathf.Clamp(amount, 0f, 1f);
        PlayerPrefs.SetFloat("VOLUME." + label.ToUpper(), amount);
        ApplySettings();
    }

    public void SetResolution(int x, int y, bool fullscreen){
        PlayerPrefs.SetInt("SCREEN.WIDTH", x);
        PlayerPrefs.SetInt("SCREEN.HEIGHT", y);
        int full_val = 0;
        if(fullscreen)
            full_val = 1;
        PlayerPrefs.SetInt("FULLSCREEN", full_val);
        ApplySettings();
    }
}