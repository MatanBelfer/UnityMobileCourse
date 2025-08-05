using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinShopItem : MonoBehaviour
{
    /// <summary>
    /// Data of a shop item, used in ShopManager
    /// </summary>
    public string identifier { get; private set; } //used in ShopManager
    private bool isPurchased;
    private bool isEquipped;
    private SkinAsset asset;

    [Header("Children")] 
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text buyEquipText;
    [SerializeField] private Button buyEquipBtn;
    
    public string skinName => nameText.text;
    
    public void SetItemData(string skinIdentifier, SkinAsset asset, bool isPurchased, bool isEquipped)
    {
        this.asset = asset;
        identifier = skinIdentifier;
        this.isPurchased = isPurchased;
        this.isEquipped = isEquipped;
        //print(asset.displayName);
        image.sprite = asset.sprite;
        nameText.text = asset.displayName;
        
        UpdateBuyEquipText();
    }

    /// <summary>
    /// Updates the contextual button text
    /// </summary>
    private void UpdateBuyEquipText()
    {
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

    private void Start()
    {
        buyEquipBtn.onClick.AddListener(buyEquipBtnClicked);
    }

    /// <summary>
    /// Context dependant button. if not purchased, try purchase. if purchased but not equipped, equip.
    /// If equipped, unequip
    /// </summary>
    private void buyEquipBtnClicked()
    {
        ShopManager shopManager = ManagersLoader.Shop;
        
        if (!isPurchased)
        {
            shopManager.TryPurchase(this);
        }
        else if (!isEquipped)
        {
            shopManager.Equip(this);
        }
        else
        {
            shopManager.Unequip(this);
        }
    }

    public void UpdateShopItemState(bool isPurchased, bool isEquipped)
    {
        this.isPurchased = isPurchased;
        this.isEquipped = isEquipped;
        UpdateBuyEquipText();
    }
}
