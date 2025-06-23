using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Linq;

public class ShopManager : BaseManager
{
    [Header("Data")] [SerializeField] private ShopAssetCollection skinCollection;
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
        SaveShopItems();
    }

    public void Start()
    {
        InstantiateShopItems();
        if (testMode && title != null)
        {
            try
            {
                if (shopItems != null && shopItems.Length > 0)
                {
                    title.text = string.Join(", ", shopItems.Select(item => item.skinName).ToArray());
                }
                else
                {
                    title.text = "No Shop Items Found";
                }
            }
            catch
            {
                title.text = "Skins Not Found";
            }
        }
    }

    private void LoadShopItems()
    {
        if (skinCollection == null)
        {
            Debug.LogError("SkinCollection is not assigned in ShopManager!");
            return;
        }

        Resources.LoadAll("Skins");
        skins = skinCollection.skinDictionary;        string path = Application.persistentDataPath + saveDataPath;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            savedPurchasedSkinsAreEquipped = JsonUtility.FromJson<Dictionary<string, bool>>(json);
//            print($"loaded shop items from {path}");
        }
        else
        {
            savedPurchasedSkinsAreEquipped = new Dictionary<string, bool>();
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

    public void SaveShopItems()
    {
        if (savedPurchasedSkinsAreEquipped != null)
        {
            string json = JsonUtility.ToJson(savedPurchasedSkinsAreEquipped);
            string path = Application.persistentDataPath + saveDataPath;
            File.WriteAllText(path, json);
            print($"saved purchased shop items to {path}");
        }
    }
}