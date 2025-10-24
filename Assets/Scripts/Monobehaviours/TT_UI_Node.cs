using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HenrysTechUtils;

public class TT_UI_Node : MonoBehaviour{

    [Header("Icons")]
    [SerializeField] Image Icon;
    [SerializeField] Image Highlight;

    [Header("Connecting Lines")]
    [SerializeField] RectTransform LineRect;
    [SerializeField] Image[] Connections;
    [SerializeField] GameObject LeftConnection;
    [SerializeField] GameObject MiddleConnection;
    [SerializeField] GameObject RightConnection;

    [Header("Colours")]
    [SerializeField] Color OwnedColour;
    [SerializeField] Color AvailableColour;
    [SerializeField] Color UnavailableColour;
    [SerializeField] Color EnabledConnectionColour;
    [SerializeField] Color DisabledConnectionColour;

    const float layer_height = 160;
    const float width_unit = 150;
    TechNode node;
    TechTreeUI manager;

    public float x_position {get; private set;}
    
    public void Setup(TechNode _node, TechTreeUI manag, int depth, int girth, int index, float parent_x){
        node = _node;
        manager = manag;
        SetConnections(node.ChildrenCount());
        SetBounds(depth, girth, index, parent_x);
        RefreshColours();
    }

    public void RefreshColours(){
        Color highlight_colour = UnavailableColour;
        Color connection_colour = DisabledConnectionColour;

        if(node.unlocked){
            highlight_colour = OwnedColour;
            connection_colour = EnabledConnectionColour;
        }
        else if(node.Available())
            highlight_colour = AvailableColour;

        Highlight.color = highlight_colour;
        foreach(Image image in Connections)
            image.color = connection_colour;
    }

    public void Unlock(){
        node.Unlock();
        manager.Refresh();
    }

    void SetBounds(int depth, int girth, int index, float parent_x){

        x_position = 0f;

        int relative_mult = 0;
        if(node.SiblingCount() > 0){
            if(node.SiblingCount() == 1){
                if(index == 0)
                    relative_mult = -1;
                else
                    relative_mult = 1;
            }
            else if(node.SiblingCount() == 2)
                relative_mult = index - 1;
        }

        x_position = relative_mult * width_unit * ((float)node.ParentWidth() / 2f);
        x_position += parent_x;

        this.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(x_position, layer_height *-depth);
        LineRect.sizeDelta = new Vector2(girth * width_unit, layer_height);
    }

    void SetConnections(int connections){
        LineRect.gameObject.SetActive(connections > 0);
        LeftConnection.gameObject.SetActive(connections == 2 || connections == 3);
        MiddleConnection.gameObject.SetActive(connections == 1 || connections == 3);
        RightConnection.gameObject.SetActive(connections == 2 || connections == 3);
    }
}
