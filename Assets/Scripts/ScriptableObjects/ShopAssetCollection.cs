using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ShopAssetCollection", menuName = "Scriptable Objects/ShopAssetCollection")]
public class ShopAssetCollection : ScriptableObject
{
    private Dictionary<string, SkinAsset> _skinDictionary;
    public Dictionary<string, SkinAsset> skinDictionary
    {
        get
        {
            if (_skinDictionary == null) return GenerateDictionary();
            return _skinDictionary;
        }
    }

    [SerializeField] private SkinAsset[] _skins;
    private string[] _skinAssetNames;

    public void OnValidate()
    {
        _skinAssetNames = _skins.Select(s => s.name).ToArray();
    }

    public Dictionary<string, SkinAsset> GenerateDictionary()
    {
        _skinDictionary = new Dictionary<string, SkinAsset>();
        foreach (var name in _skinAssetNames)
        {
            _skinDictionary.Add(name,Resources.Load<SkinAsset>("Skins/" + name));
        }
        
        return _skinDictionary;
    }
}
