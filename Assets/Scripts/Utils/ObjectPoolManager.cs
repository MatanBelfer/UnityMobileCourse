using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private PoolSettings[] poolSettings;
    private Dictionary<string, Queue<GameObject>> pools = new();
    private Dictionary<string, HashSet<GameObject>> activeObjects = new();

    [Serializable]
    public class PoolSettings
    {
        public string poolName;
        public int poolSize;
        public GameObject prefab;
    }

    public static ObjectPoolManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePools();
    }

    public void InitializePools()
    {
        pools.Clear();
        activeObjects.Clear();

        foreach (PoolSettings settings in poolSettings)
        {
            string name = settings.poolName;
            int poolSize = settings.poolSize;
            GameObject prefab = settings.prefab;

            pools[name] = new Queue<GameObject>();
            activeObjects[name] = new HashSet<GameObject>();
            AddItemsToPool(poolSize, prefab, name);
        }
    }

    private void AddItemsToPool(int numItems, GameObject prefab, string poolName)
    {
        for (int i = 0; i < numItems; i++)
        {
            GameObject newobj = Instantiate(prefab, transform);
            newobj.SetActive(false);

            ObjectPoolInterface @interface = newobj.GetComponent<ObjectPoolInterface>();
            if (@interface != null)
            {
                @interface.poolName = poolName;
                @interface.objectPoolManager = Instance;
            }

            pools[poolName].Enqueue(newobj);
        }
    }

    private void CheckPoolExists(string poolName)
    {
        if (!pools.ContainsKey(poolName))
        {
            throw new Exception($"There is no pool named \"{poolName}\"");
        }
    }

    public string[] GetPoolNames()
    {
        return pools.Keys.ToArray();
    }

    public GameObject GetFromPool(string poolName)
    {
        CheckPoolExists(poolName);
        Queue<GameObject> pool = pools[poolName];

        GameObject obj;
        if (pool.Count <= 0)
        {
            Debug.LogWarning($"Pool \"{poolName}\" has ran out of items. Creating more...");
            PoolSettings settings = poolSettings.First(s => s.poolName == poolName);
            int currentSize = settings.poolSize > 0 ? settings.poolSize : 1;
            AddItemsToPool(currentSize, settings.prefab, poolName);
            settings.poolSize *= 2;
        }

        obj = pool.Dequeue();

        // Validate object before returning
        if (obj == null)
        {
            Debug.LogWarning($"Found null object in pool {poolName}, creating new one");
            obj = Instantiate(poolSettings.First(s => s.poolName == poolName).prefab, transform);
            var interface_ = obj.GetComponent<ObjectPoolInterface>();
            if (interface_ != null)
            {
                interface_.poolName = poolName;
                interface_.objectPoolManager = Instance;
            }
        }

        obj.transform.SetParent(null);
        obj.SetActive(true);
        activeObjects[poolName].Add(obj);
        return obj;
    }

    public void InsertToPool(string poolName, GameObject obj)
    {
        if (obj == null) return;
        CheckPoolExists(poolName);

        // Remove from current parent first
        Transform originalParent = obj.transform.parent;
        if (originalParent != null)
        {
            obj.transform.SetParent(null);
        }
        
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        pools[poolName].Enqueue(obj);
        activeObjects[poolName].Remove(obj);
    }

    public void OnSceneUnloaded()
    {
        // Return all active objects to their pools
        foreach (var poolName in pools.Keys)
        {
            var activeSet = activeObjects[poolName];
            foreach (var obj in activeSet.ToList())
            {
                if (obj != null)
                {
                    InsertToPool(poolName, obj);
                }
            }

            activeSet.Clear();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}