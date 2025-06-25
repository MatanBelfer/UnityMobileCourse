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
            if (!_sprite)
            {
                Debug.Log($"{name} is trying to load sprite: {spriteName}...");
                _sprite = Resources.Load<Sprite>("Skins/" + spriteName);
                string successs = _sprite ? "succeeded" : "failed";
                Debug.Log($"load {successs} for {name}");
                
            }
            return _sprite;
        }
    }
    private void OnValidate()
    {
        spriteName = _sprite.name;
    }


    public int price {get => _price;}
}
