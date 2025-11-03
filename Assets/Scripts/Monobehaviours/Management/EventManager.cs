using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenericUtils;
using EventUtils;
using System.Linq;

public class EventManager : MonoBehaviour{

    WorldEventManager manager;
    List<Window> turn_sensitive_windows = new List<Window>();

    [Header("- References -")]
    [SerializeField] GameplayManager _GameplayManager;

    [Header(" - Alert UI - ")]
    [SerializeField] Transform WindowHook;
    [SerializeField] GameObject NewspaperPrefab;
    [SerializeField] GameObject PeacePrefab;
    [SerializeField] GameObject PrivateMessagePrefab;

    public void Setup() {
        manager = new WorldEventManager(this);
    }

    public void Tick(){
        manager.RunEvents();
    }

    public void Add(WorldEvent new_event){
        manager.Add(new_event);
    }

    // UI Alerts //

    public void TestWar(Faction our_fact, Faction target_fact){
        if(turn_sensitive_windows.Count == 0)
            return;

        List<PeaceOfferingWindow> peace_offerings = turn_sensitive_windows.OfType<PeaceOfferingWindow>().ToList();
        if(peace_offerings.Count == 0)
            return;
        
        foreach(PeaceOfferingWindow peace in peace_offerings){
            peace.TestWar(our_fact, target_fact);
        }
    }

    public void Message(MessageContents message){
        switch(message.Type()){
            case "NEWSPAPER":
                CreateWindow(NewspaperPrefab).GetComponent<NewspaperWindow>().Setup(message);
                break;
            case "PEACE":
                PeaceOfferingWindow pow = CreateWindow(PeacePrefab).GetComponent<PeaceOfferingWindow>();
                pow.Setup(_GameplayManager, message);
                turn_sensitive_windows.Add(pow);
                break;
            case "PRIVATE MESSAGE":
                MessageDisplayWindow mdw = CreateWindow(PrivateMessagePrefab).GetComponent<MessageDisplayWindow>();
                mdw.Setup(_GameplayManager.LocalUsername(), message);
                break;
        }
    }

    public void CleanTurnSensitiveAlerts(){
        if(turn_sensitive_windows.Count == 0)
            return;
        foreach(Window w in turn_sensitive_windows){
            if(w != null)
                w.Close();
        }
        turn_sensitive_windows = new List<Window>();
    }

    GameObject CreateWindow(GameObject prefab){
        GameObject window = GameObject.Instantiate(prefab);
        window.transform.SetParent(WindowHook);
        window.transform.localPosition = Vector3.zero;
        window.transform.localScale = new Vector3(1,1,1);
        return window;
    }
}