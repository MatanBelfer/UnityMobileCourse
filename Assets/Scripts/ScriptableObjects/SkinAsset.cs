using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SkinAsset", menuName = "Scriptable Objects/Skin Asset")]
public class SkinAsset : ScriptableObject
{
    [SerializeField] private string _displayName;
    public string displayName {get => _displayName;}
    private string spriteName;
    [SerializeField] private int _price;
    public int price => _price;
    [SerializeField] private Sprite _sprite;
    public Sprite sprite
    {
        get
        {
            if (!_sprite)
            {
                _sprite = Resources.Load<Sprite>("Skins/" + spriteName);
            }
            return _sprite;
        }
    }
    
    private void OnValidate()
    {
        spriteName = _sprite.name;
    }
}
