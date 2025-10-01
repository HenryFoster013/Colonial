using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Nameset))]
public class NamesetEditor : Editor{
    public override void OnInspectorGUI(){
        DrawDefaultInspector();
        GUILayout.Space(10);
        Nameset nameset = (Nameset)target;
        if(GUILayout.Button("Regenerate Arrays")){
            nameset.RegenerateArrays();
            EditorUtility.SetDirty(nameset);
        }
    }
}
