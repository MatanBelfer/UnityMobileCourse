using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnClick : MonoBehaviour
{
    [SerializeField] private ObjectPoolManager objectPoolManager;
    [SerializeField] private string[] poolNames;
    [SerializeField] private string currentPool;
    private Dictionary<KeyCode, int> keys = new();

    private void Start()
    {
        poolNames = objectPoolManager.GetPoolNames();
        currentPool = poolNames[0];
        
        print(poolNames);
        
        keys.Add(KeyCode.Alpha1, 0);
        keys.Add(KeyCode.Alpha2, 1);
        keys.Add(KeyCode.Alpha3, 2);
        keys.Add(KeyCode.Alpha4, 3);
        keys.Add(KeyCode.Alpha5, 4);
        keys.Add(KeyCode.Alpha6, 5);
        keys.Add(KeyCode.Alpha7, 6);
        keys.Add(KeyCode.Alpha8, 7);
        keys.Add(KeyCode.Alpha9, 8);
    }

    private void Update()
    {
        foreach (KeyValuePair<KeyCode,int> key in keys)
        {
            if (Input.GetKeyDown(key.Key))
            {
                if (key.Value >= poolNames.Length) return;
                
                currentPool = poolNames[key.Value];
                return;
            }
        }
    }
    
    private void OnMouseDown()
    {
        Vector3 clickedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        clickedPos.z = 0;

        GameObject newobj = objectPoolManager.GetFromPool(currentPool);
        newobj.transform.position = clickedPos;
    }
}
