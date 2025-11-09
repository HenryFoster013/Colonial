using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static GenericUtils;

public class QuickQuit : MonoBehaviour{

    [Header("Misc")]
    [SerializeField] SoundEffectLookup SFX_Lookup;
    
    [Header("Scenes")]
    [SerializeField] string[] QuitToDesktop;
    [SerializeField] string[] ConnectiveDisconnect;

    [Header("UI References")]
    [SerializeField] GameObject ProgressHolder;
    [SerializeField] Slider ProgressSlider;
    [SerializeField] GameObject DefaultColour;
    [SerializeField] GameObject ReadyColour;
    
    float esc_down_stamp, held_time;
    bool esc_pressed;

    const float activate_time = 0.333f;
    const float hold_time = 0.9f;

    // Instantiation
    public void Setup(){
        DontDestroyOnLoad(this.gameObject);
    }

    // Per Frame
    void Update(){
        EscapeManagement();
        UpdateUI();
    }

    void EscapeManagement(){
        if(Input.GetKeyDown(KeyCode.Escape))
            EscapeDown();
        if(Input.GetKeyUp(KeyCode.Escape))
            EscapeUp();
        held_time = Time.time - esc_down_stamp;
    }

    void UpdateUI(){
        ProgressHolder.SetActive(held_time > activate_time && esc_pressed);
        ProgressSlider.value = (held_time - activate_time) / hold_time;

        bool ready = held_time > activate_time + hold_time;
        DefaultColour.SetActive(!ready);
        ReadyColour.SetActive(ready);
    }

    // Presses
    void EscapeDown(){
        esc_pressed = true;
        esc_down_stamp = Time.time;
    }

    void EscapeUp(){
        if(!esc_pressed)
            return;
        esc_pressed = false;

        if(held_time > hold_time + activate_time)
            CallQuits();
        else if(held_time < activate_time)
            RegularEscape();
    }

    // End Functions
    void RegularEscape(){
        string scene_name = SceneManager.GetActiveScene().name.ToUpper();
        print(scene_name);

        if(scene_name == "TITLE SCREEN"){
            print("WIGGER");
            GameObject.FindGameObjectWithTag("Player").GetComponent<MainMenu>().SettingsMenu();
            return;
        }

        if(scene_name == "ONLINE GAMEPLAY"){
            //GameObject.FindGameObjectWithTag("Player").GetComponent<MainMenu>().SettingsMenu();
            return;
        }
    }

    void CallQuits(){
        string scene_name = SceneManager.GetActiveScene().name.ToUpper();

        for(int i = 0; i < QuitToDesktop.Length; i++){
            if(QuitToDesktop[i].ToUpper() == scene_name){
                Desktop();
                return;
            }
        }

        for(int i = 0; i < ConnectiveDisconnect.Length; i++){
            if(ConnectiveDisconnect[i].ToUpper() == scene_name){
                Disconnect();
                return;
            }
        }

        MainMenu();
    }

    void Desktop(){
        Application.Quit();
    }

    void Disconnect(){
        print("Connective Disconnect");
    }

    void MainMenu(){
        print("Main Menu");
        PlaySFX("UI_2", SFX_Lookup);
        SceneManager.LoadScene("Title Screen");
    }
}