using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundColouring : MonoBehaviour
{
    [SerializeField] RawImage Base;
    [SerializeField] RawImage Shade;
    [SerializeField] bool OverrideSaved = false;

    Color base_colour;
    Color current_base_colour;
    Color shade_colour;
    Color current_shade_colour;

    void Start(){
        SetTargets(Base.color, Shade.color);

        if(OverrideSaved)
            SaveColours(base_colour, shade_colour);

        current_base_colour = PrefsToColour("BG_BaseColour");
        current_shade_colour = PrefsToColour("BG_ShadeColour");

        Base.color = current_base_colour;
        Shade.color = current_shade_colour;
    }
    
    void Update(){
        current_base_colour = Color.Lerp(current_base_colour, base_colour, Time.deltaTime * 10f);
        current_shade_colour = Color.Lerp(current_shade_colour, shade_colour, Time.deltaTime * 10f);
        Base.color = current_base_colour;
        Shade.color = current_shade_colour;
    }

    public void SetTargets(Color based, Color shade){
        base_colour = based;
        shade_colour = shade;
    }

    public void Save(){
        SaveColours(current_base_colour, current_shade_colour);
    }

    Color PrefsToColour(string reference){
        if(PlayerPrefs.GetString(reference) == "")
            SaveColours(base_colour, shade_colour);
        
        return StringToColour(PlayerPrefs.GetString(reference));
    }

    Color StringToColour(string colour_string){
        string[] cp = colour_string.Split(','); // "Colour Parts"
        return new Color(float.Parse(cp[0]), float.Parse(cp[1]), float.Parse(cp[2]), float.Parse(cp[3]));
    }

    void SaveColours(Color based, Color shade){
        PlayerPrefs.SetString("BG_BaseColour", based.r.ToString() + "," + based.g.ToString() + "," + based.b.ToString() + "," + based.a.ToString());
        PlayerPrefs.SetString("BG_ShadeColour", shade.r.ToString() + "," + shade.g.ToString() + "," + shade.b.ToString() + "," + shade.a.ToString());
    }
}
