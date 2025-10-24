using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HenrysTechUtils;

public class TT_UI_Node : MonoBehaviour{

    [SerializeField] Image DisplayImage;
    [SerializeField] RectTransform LineRect;
    [SerializeField] GameObject LeftConnection;
    [SerializeField] GameObject MiddleConnection;
    [SerializeField] GameObject RightConnection;

    const float layer_height = 160;
    const float width_unit = 150;

    public float x_position {get; private set;}
    
    public void Setup(TechNode node, int depth, int girth, int index, float parent_x){
        SetConnections(node.ChildrenCount());
        SetBounds(node, depth, girth, index, parent_x);
    }

    void SetBounds(TechNode node, int depth, int girth, int index, float parent_x){

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
