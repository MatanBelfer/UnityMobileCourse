#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SkinAsset))]
public class SkinAssetEditor : Editor
{
    private void OnValidate()
    {
        var skinAsset = (SkinAsset)target;
        skinAsset.SetSpriteName(skinAsset.sprite.name);
    }
}
#endif