using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenericUtils;
using EventUtils;

public class EventManager : MonoBehaviour{

    WorldEventManager manager;

    [Header(" - Alert UI - ")]
    [SerializeField] Transform WindowHook;
    [SerializeField] GameObject NewspaperPrefab;

    
    public void Setup() {
        manager = new WorldEventManager(this);
    }

    public void Tick(int turn, int player){
        manager.Tick(turn, player);
    }

    // UI Alerts //

    public void Message(MessageContents message){
        switch(message.Type()){
            case "NEWSPAPER":
                CreateWindow(NewspaperPrefab).GetComponent<NewspaperWindow>().Setup(message);
                break;
        }
    }

    GameObject CreateWindow(GameObject prefab){
        GameObject window = GameObject.Instantiate(prefab);
        window.transform.SetParent(WindowHook);
        window.transform.position = Vector3.zero;
        window.transform.localScale = new Vector3(1,1,1);
        return window;
    }
}