using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Linq;

public class ShopManager : BaseManager
{
    [Header("Data")]
    private Dictionary<string, SkinAsset> skins; //all existing skins
    private Dictionary<string, bool> savedPurchasedSkinsAreEquipped; // <skin name, is equipped> from json
    private SkinShopItem equippedSkinShopItem; //shop item of the currently equipped skin
    public SkinAsset equippedSkinAsset { get; private set; } //the skin that is currently equipped
    [SerializeField] private int money;
    private const string saveDataPath = "PurchasedSkins.json";
    private SkinShopItem[] shopItems;

    [Header("UI")] 
    [SerializeField] [Tooltip("The object meant to be the parent of the shop items")] public GameObject shopPanel;
    [SerializeField] private GameObject shopItemPrefab;
    public TMP_Text moneyText;
    
    [Header("Testing")] [SerializeField] private bool testMode;
    [SerializeField] private TMP_Text title;

    public void Awake()
    {
        base.Awake();
    }

    protected override void OnInitialize()
    {
        // print("Shop Manager initialized");
        LoadShopItems();
    }

    protected override void OnReset()
    {
        // Reset shop state if needed
        LoadShopItems();
    }

    protected override void OnCleanup()
    {
        // Save any pending changes
        SavePlayerShopData();
    }

    public void ReinstantiateShopItems()
    {
        InstantiateShopItems();
        UpdateMoneyText();
    }

    // private void Update()
    // {
    //     print(equippedSkinAsset?.displayName ?? "no skin");
    // }

    private void Start()
    {
        ManagersLoader.Game.OnRestartLevel += () => money += ManagersLoader.Game.currentScore;
        ManagersLoader.Game.OnExitToMainMenu += () => money += ManagersLoader.Game.currentScore;
    }

    private void LoadShopItems()
    {
        var skinAssetArray = Resources.LoadAll<SkinAsset>("Skins");
        skins = skinAssetArray.ToDictionary(skin => skin.name);
        string path = Application.persistentDataPath + saveDataPath;
        PlayerShopData playerShopData = null;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            playerShopData = JsonUtility.FromJson<PlayerShopData>(json);
            savedPurchasedSkinsAreEquipped = playerShopData.purchasedSkinsAreEquipped;
            money = playerShopData.money;
        }
        if (savedPurchasedSkinsAreEquipped == null)
        {
            Debug.LogWarning($"No saved shop data found at {path}");
            savedPurchasedSkinsAreEquipped = new Dictionary<string, bool>();
            money = 0;
        }
        money = 500;
        print("gave 500 money");
    }

    private void InstantiateShopItems()
    {
        print("Instantiating shop itmes");
        if (skins == null || skins.Count == 0)
        {
            Debug.LogWarning("No skins available to instantiate shop items");
            return;
        }

        if (shopPanel == null)
        {
            Debug.LogError("Shop panel is not assigned!");
            return;
        }

        if (shopItemPrefab == null)
        {
            Debug.LogError("Shop item prefab is not assigned!");
            return;
        }

        shopItems = new SkinShopItem[skins.Count];
//        print(1);
        int index = 0;
        foreach (var skin in skins)
        {
//            print(skin.Value.displayName);
            GameObject newItem = Instantiate(shopItemPrefab, shopPanel.transform);
            SkinShopItem itemData = newItem.GetComponent<SkinShopItem>();
  //          print($"Instantiating shop item {skin.Key}");
            
            if (itemData != null)
            {
                bool purchased = savedPurchasedSkinsAreEquipped.ContainsKey(skin.Key);
                bool equipped = purchased ? savedPurchasedSkinsAreEquipped[skin.Key] : false;
                itemData.SetItemData(skin.Key, skin.Value, purchased, equipped);

                if (equipped)
                {
                    equippedSkinShopItem = itemData;
                    equippedSkinAsset = skin.Value;
                }
        
                shopItems[index] = itemData;
            }
            index++;
        }
    }

    private void OnApplicationQuit()
    {
        SavePlayerShopData();
    }

    private void SavePlayerShopData()
    {
        if (savedPurchasedSkinsAreEquipped != null)
        {
            string json = JsonUtility.ToJson(new PlayerShopData(money, savedPurchasedSkinsAreEquipped) );
            string path = Application.persistentDataPath + saveDataPath;
            File.WriteAllText(path, json);
        }
    }

    private class PlayerShopData
    {
        public int money;
        public Dictionary<string, bool> purchasedSkinsAreEquipped;
        
        public PlayerShopData(int money, Dictionary<string, bool> purchasedSkinsAreEquipped)
        {
            this.money = money;
            this.purchasedSkinsAreEquipped = purchasedSkinsAreEquipped;
        }
    }

    public void AddMoney(int rewardAmount)
    {
        money += rewardAmount;
    }

    private void UpdateMoneyText()
    {
        moneyText.text = $"Money: {money}";
    }

    /// <summary>
    /// Attempts purchase of an item
    /// </summary>
    /// <param name="shopItem">name in "skins" dictionary</param>
    public void TryPurchase(SkinShopItem shopItem)
    {
        string identifier = shopItem.identifier;
        int price = skins[identifier].price;

        if (money < price)
        {
            print("Not enough money");
            return;
        }
        
        DoPurchase(identifier);
        Equip(shopItem);

        CheckItemState(identifier, out bool isPurchased, out bool isEquipped);
        shopItem.UpdateShopItemState(isPurchased, isEquipped);
    }

    /// <summary>
    /// Either equips or unequips item;
    /// </summary>
    public void Equip(SkinShopItem shopItem)
    {
        //first unequip the equipped item
        if (equippedSkinShopItem) Unequip(equippedSkinShopItem);
        
        //then equip this one
        if (!savedPurchasedSkinsAreEquipped.ContainsKey(shopItem.identifier))
        {
            Debug.LogWarning($"Attempted equip of a non purchased skin {shopItem.identifier}");
        }
        else
        {
            savedPurchasedSkinsAreEquipped[shopItem.identifier] = true;
        }

        equippedSkinShopItem = shopItem;
        equippedSkinAsset = skins[shopItem.identifier];
        
        CheckItemState(shopItem.identifier, out bool isPurchased, out bool isEquipped);
        shopItem.UpdateShopItemState(isPurchased, isEquipped);
    }

    public void Unequip(SkinShopItem shopItem)
    {
        equippedSkinShopItem = null;
        equippedSkinAsset = null;
        savedPurchasedSkinsAreEquipped[shopItem.identifier] = false;
        
        CheckItemState(shopItem.identifier, out bool isPurchased, out bool isEquipped);
        shopItem.UpdateShopItemState(isPurchased, isEquipped);
    }

    private void CheckItemState(string identifier, out bool isPurchased, out bool isEquipped)
    {
        isPurchased = savedPurchasedSkinsAreEquipped.ContainsKey(identifier);
        if (!isPurchased)
        {
            isEquipped = false;
            return;
        }
        isEquipped = savedPurchasedSkinsAreEquipped[identifier];
    }
    
    private void DoPurchase(string skinIdentifier)
    {
        money -= skins[skinIdentifier].price;
        UpdateMoneyText();
        savedPurchasedSkinsAreEquipped[skinIdentifier] = false; //saves it as bought 
    }
}