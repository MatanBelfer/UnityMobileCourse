#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShopAssetCollection))]
public class ShopAssetCollectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (ShopAssetCollection)target;

        if (GUILayout.Button("Generate Dictionary"))
        {
            script.GenerateDictionary();
        }
    }
}
#endif