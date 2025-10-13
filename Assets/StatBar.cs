using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    [SerializeField] Transform Holder;
    [SerializeField] GameObject Unit;
    Color off_colour;

    public void Refresh(int max, int amount){
        Color off_colour = Holder.parent.GetComponent<Image>().color;
        foreach(Transform t in Holder)
            GameObject.Destroy(t.gameObject);
        for(int i = 1; i <= max; i++){
            GameObject new_unit = GameObject.Instantiate(Unit);
            new_unit.transform.SetParent(Holder);
            RectTransform rect = new_unit.GetComponent<RectTransform>();
            rect.localScale = new Vector3(1,1,1); 
            rect.localPosition  = Vector3.zero;

            if(i > amount)
                new_unit.GetComponent<Image>().color = off_colour;
            
            float full_size = 100f;
            float width = full_size / max;
            float left = width * (i - 1);
            rect.offsetMin = new Vector2(left, 0f); // Left, Bottom
            rect.offsetMax = new Vector2(left + width - full_size, 0f); // Right, Top

            new_unit.SetActive(true);
        }
    }
}
