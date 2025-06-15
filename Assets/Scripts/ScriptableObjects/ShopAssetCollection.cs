using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShopAssetCollection", menuName = "Scriptable Objects/ShopAssetCollection")]
public class ShopAssetCollection : ScriptableObject
{
    public Dictionary<string, SkinAsset> skinDictionary { get; private set; }
    [SerializeField] private SkinAsset[] _skins;

    public void OnValidate()
    {
        GenerateDictionary();
    }

    public void GenerateDictionary()
    {
        skinDictionary = new Dictionary<string, SkinAsset>();
        foreach (var skin in _skins)
        {
            skinDictionary.Add(skin.skinName, skin);
        }
    }
}
