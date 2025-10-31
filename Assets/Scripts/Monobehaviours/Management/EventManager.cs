using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenericUtils;
using EventUtils;

public class EventManager : MonoBehaviour{

    WorldEventManager manager;

    [Header("- References -")]
    [SerializeField] GameplayManager _GamePlayManager;

    [Header(" - Alert UI - ")]
    [SerializeField] Transform WindowHook;
    [SerializeField] GameObject NewspaperPrefab;
    [SerializeField] GameObject PeacePrefab;

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

    public void Message(MessageContents message){
        switch(message.Type()){
            case "NEWSPAPER":
                CreateWindow(NewspaperPrefab).GetComponent<NewspaperWindow>().Setup(message);
                break;
            case "PEACE":
                CreateWindow(PeacePrefab).GetComponent<PeaceOfferingWindow>().Setup(_GamePlayManager, message);
                break;
        }
    }

    GameObject CreateWindow(GameObject prefab){
        GameObject window = GameObject.Instantiate(prefab);
        window.transform.SetParent(WindowHook);
        window.transform.localPosition = Vector3.zero;
        window.transform.localScale = new Vector3(1,1,1);
        return window;
    }
}