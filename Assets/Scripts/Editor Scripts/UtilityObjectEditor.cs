#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UtilityObject))]
public class UtilityObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (UtilityObject)target;

        if (GUILayout.Button("Reset Player Prefs"))
        {
            script.ResetPlayerPrefs();
        }
    }
}
#endif