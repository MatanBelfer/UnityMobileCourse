using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinShopItem : MonoBehaviour
{
    // private SkinAsset asset;
    private bool isPurchased;
    private bool isEquipped;

    [Header("Children")] 
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text buyEquipText;
    
    public string skinName => nameText.text;
    
    public void SetItemData(SkinAsset asset, bool isPurchased, bool isEquipped)
    {
        // this.asset = asset;
        this.isPurchased = isPurchased;
        this.isEquipped = isEquipped;
        //print(asset.displayName);
        image.sprite = asset.sprite;
        nameText.text = asset.displayName;
        
        switch (isPurchased, isEquipped)
        {
            case (true, true):
                buyEquipText.text = "Equipped";
                break;
            case (true, false):
                buyEquipText.text = "Equip";
                break;
            case (false, false):
                buyEquipText.text = $"Buy: {asset.price}";
                break;
            case (false, true):
                Debug.LogError($"{gameObject.name} is equipped but not purchased");
                break;
        }
    }
}
