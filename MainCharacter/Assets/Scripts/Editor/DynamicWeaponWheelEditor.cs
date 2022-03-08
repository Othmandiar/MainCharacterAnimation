using UnityEngine;
using System.Collections;
using UnityEditor;
[CustomEditor(typeof(DynamicWheel))]
public class DynamicWheelEditor : Editor {
   
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        //DynamicWheel t = (DynamicWheel)target;
        //if (GUILayout.Button("Generate Wheel")) {
        //    t.GenerateWheel();
        //}
    }
}