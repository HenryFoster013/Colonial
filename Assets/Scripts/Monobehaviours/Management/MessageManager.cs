using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenericUtils;
using EventUtils;

public class EventRenderer : MonoBehaviour{

    WorldEventManager manager;

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

    GameObject CreateWindow(gameObject prefab){
        GameObject window = GameObject.Insantiate(prefab);
        window.SetParent(WindowHook);
        window.transform.position = Vector3.zero;
        window.transform.localScale = new Vector3(1,1,1);
    }
}