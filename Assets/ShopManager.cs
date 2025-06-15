using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ShopManager : MonoBehaviour
{
	[Header("Data")]
	[SerializeField] private ShopAssetCollection skinCollection;
	private Dictionary<string, SkinAsset> skins;
	private Dictionary<string, bool> savedPurchasedSkinsAreEquipped; // <skin name, is equipped> from json
    [SerializeField] private int money;
    private const string saveDataPath = "PurchasedSkins.json";
    private SkinShopItem[] shopItems;
    
    [Header("UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject shopItemPrefab;

    public void Awake()
    {
	    skins = skinCollection.skinDictionary;
	    LoadShopItems();
    }

    public void Start()
    {
	    InstantiateShopItems();
    }

    private void LoadShopItems()
    {
	    string path = Application.persistentDataPath + saveDataPath;
	    if (File.Exists(path))
	    {
		    string json = File.ReadAllText(path);
		    savedPurchasedSkinsAreEquipped = JsonUtility.FromJson<Dictionary<string,bool>>(json);
		    print($"loaded shop items from {path}");
	    }
	    else
	    {
		    savedPurchasedSkinsAreEquipped = new Dictionary<string,bool>();
	    }
    }

    private void InstantiateShopItems()
    {
	    shopItems = new SkinShopItem[skins.Count];
	    int index = 0;
	    foreach (var skin in skins)
	    {
		    GameObject newItem = Instantiate(shopItemPrefab, shopPanel.transform);
		    SkinShopItem itemData = newItem.GetComponent<SkinShopItem>();;
		    bool purchased = savedPurchasedSkinsAreEquipped.ContainsKey(skin.Key);
		    bool equipped = purchased ? savedPurchasedSkinsAreEquipped[skin.Key] : false;
		    itemData.SetItemData(skin.Value, purchased, equipped);
		    
		    shopItems[index] = itemData;
		    index++;
	    }
    }

    public void SaveShopItems()
    {
	    string json = JsonUtility.ToJson(savedPurchasedSkinsAreEquipped);
	    string path = Application.persistentDataPath + saveDataPath;
	    File.WriteAllText(path, json);
	    print($"saved purchased shop items to {path}");
    }
}
