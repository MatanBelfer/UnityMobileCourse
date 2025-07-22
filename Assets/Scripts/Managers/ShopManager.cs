using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Linq;

public class ShopManager : BaseManager
{
    [Header("Data")]
    private Dictionary<string, SkinAsset> skins;
    private Dictionary<string, bool> savedPurchasedSkinsAreEquipped; // <skin name, is equipped> from json
    [SerializeField] private int money;
    private const string saveDataPath = "PurchasedSkins.json";
    private SkinShopItem[] shopItems;

    [Header("UI")] [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject shopItemPrefab;
    
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
        InstantiateShopItems();
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
//            print($"loaded shop items from {path}");
        }
        if (playerShopData == null)
        {
            savedPurchasedSkinsAreEquipped = new Dictionary<string, bool>();
            money = 0;
        }
    }

    private void InstantiateShopItems()
    {
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
        int index = 0;
        foreach (var skin in skins)
        {
            GameObject newItem = Instantiate(shopItemPrefab, shopPanel.transform);
            SkinShopItem itemData = newItem.GetComponent<SkinShopItem>();
            
            if (itemData != null)
            {
                bool purchased = savedPurchasedSkinsAreEquipped.ContainsKey(skin.Key);
                bool equipped = purchased ? savedPurchasedSkinsAreEquipped[skin.Key] : false;
                itemData.SetItemData(skin.Value, purchased, equipped);

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
}