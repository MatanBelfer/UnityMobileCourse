using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SkinAsset", menuName = "Scriptable Objects/Skin Asset")]
public class SkinAsset : ScriptableObject
{
    [SerializeField] private string _displayName;
    [SerializeField] private Sprite _sprite;
    private string spriteName;
    [SerializeField] private int _price;
    public string displayName {get => _displayName;}

    public Sprite sprite
    {
        get
        {
            Debug.Log(spriteName);
            if (!_sprite) _sprite = Resources.Load<Sprite>("Skins/" + spriteName);
            return _sprite;
        }
    }

    public void SetSpriteName(string name)
    {
        spriteName = name;
    }

    public int price {get => _price;}
}
