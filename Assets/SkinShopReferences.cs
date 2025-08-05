using TMPro;
using UnityEngine;

public class SkinShopReferences : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TMP_Text moneyText;
    void Start()
    {
        ShopManager shopManager = ManagersLoader.Shop;
        if (!shopManager)
        {
            Debug.LogError("ShopManager not found");
            return;
        }

        shopManager.moneyText = moneyText;
        shopManager.shopPanel = shopPanel;
        shopManager.ReinstantiateShopItems();
    }
}
