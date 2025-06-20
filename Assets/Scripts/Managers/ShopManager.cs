using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Linq;

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
    
    [Header("Testing")]
    [SerializeField] private bool testMode;
    [SerializeField] private TMP_Text title;

    public void Awake()
    {
	    LoadShopItems();
    }

    public void Start()
    {
	    InstantiateShopItems();
	    if (testMode)
	    {
		    try
		    {
			    title.text = string.Join(", ", shopItems.Select(item => item.skinName).ToArray());
		    }
		    catch
		    {
			    title.text = "Skins Not Found";
		    }
	    }
    }

    private void LoadShopItems()
    {
	    Resources.LoadAll("Skins");
	    skins = skinCollection.skinDictionary;
	    
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
