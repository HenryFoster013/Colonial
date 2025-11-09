using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BG_MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioClip Track;
    [SerializeField] GameObject NodePrefab;

    void Start(){
        SearchForTrack();
    }

    public void SearchForTrack(){

        GameObject[] all_nodes = GameObject.FindGameObjectsWithTag("Backing Track");
        if(all_nodes.Length == 0){
            CreateSong();
            return;
        }

        bool song_exists = false;
        foreach(GameObject g in all_nodes){
            BG_MusicNode node = g.GetComponent<BG_MusicNode>();
            if(!node.Validate(Track))
                node.Terminate();
            else
                song_exists = !node.Deathly();
        }

        if(!song_exists)
            CreateSong();
    }

    void CreateSong(){
        if(Track == null)
            return;
        GameObject g = GameObject.Instantiate(NodePrefab);
        g.GetComponent<BG_MusicNode>().Play(Track);
    }
}
