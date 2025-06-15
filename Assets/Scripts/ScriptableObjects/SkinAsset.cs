using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SkinAsset", menuName = "Scriptable Objects/Skin Asset")]
public class SkinAsset : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private int _price;
    public string skinName {get => _name;}
    public Sprite sprite {get => _sprite;}
    public int price {get => _price;}
}
