using System.Collections.Generic;
using System.Linq;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagersLoader : MonoBehaviour
{
    [Header("Core Managers (Persistent across scenes)")]
    [SerializeField] private BaseManager[] coreManagers = null;
    
    // Static access to core managers
    public static ObjectPoolManager Pool { get; private set; }
    public static UIManager UI { get; private set; }
    public static GameManager Game { get; private set; }
    public static InputSystemManager Input { get; private set; }
    public static AnalyticsManager Analytics { get; private set; }
    public static AudioManager Audio { get; private set; }
    public static ShopManager Shop { get; private set; }
    

    
    
    // Dictionary for dynamic scene manager access
    private static Dictionary<System.Type, BaseManager> sceneManagersDict = new Dictionary<System.Type, BaseManager>();
    private static BaseManager[] currentSceneManagers;

    public static bool IsInitialized { get; private set; }
    public static ManagersLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            if (this == Instance) return;
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize core managers immediately
        InitializeCoreManagers();

        // Subscribe to scene events AFTER core managers are ready
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // Initialize scene managers for current scene
        RefreshSceneManagers();

        IsInitialized = true;
        
        //Debug.Log("ManagersLoader initialized successfully!");
    }

    private void InitializeCoreManagers()
    {
        //Debug.Log("Initializing Core Managers...");
        
        // Find and assign core managers from the serialized array
        if (coreManagers != null && coreManagers.Length > 0)
        {
            Pool = coreManagers.OfType<ObjectPoolManager>().FirstOrDefault();
            UI = coreManagers.OfType<UIManager>().FirstOrDefault();
            Game = coreManagers.OfType<GameManager>().FirstOrDefault();
            Input = coreManagers.OfType<InputSystemManager>().FirstOrDefault();
            Analytics = coreManagers.OfType<AnalyticsManager>().FirstOrDefault();
            Audio = coreManagers.OfType<AudioManager>().FirstOrDefault();
            Shop = coreManagers.OfType<ShopManager>().FirstOrDefault();
        }
        
        // Initialize ObjectPool first (other managers depend on it)
        Pool?.InitManager();
        
        // Initialize other core managers
        foreach (var manager in coreManagers.Where(m => m != null && !(m is ObjectPoolManager)))
        {
            manager.InitManager();
        }
    }

    private void RefreshSceneManagers()
    {
        //Debug.Log("Refreshing Scene Managers...");
        
        // Clear previous references
        sceneManagersDict.Clear();

        // Find all scene managers in the current scene (exclude core managers)
        var foundManagers = FindObjectsByType<BaseManager>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(m => !IsCoreManager(m))
            .ToArray();
            
        currentSceneManagers = foundManagers;
        

        
        // Populate the dictionary and initialize
        foreach (var manager in foundManagers)
        {
            if (manager != null)
            {
                sceneManagersDict[manager.GetType()] = manager;
                manager.InitManager();
//                Debug.Log($"Initialized scene manager: {manager.GetType().Name}");
            }
        }
    }

    private bool IsCoreManager(BaseManager manager)
    {
        // Check if this manager is in our core managers array
        return coreManagers != null && coreManagers.Contains(manager);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
//        Debug.Log($"Scene loaded: {scene.name}");
        
        // Use Unity's built-in frame delay
        Invoke(nameof(RefreshSceneManagers), 0f);
    }

    private void OnSceneUnloaded(Scene scene)
    {
//        Debug.Log($"Scene unloaded: {scene.name}");
        
        // Reset core managers that need scene refresh (but don't cleanup)
        Pool?.ResetManager();
        
        // Cleanup scene managers only
        if (currentSceneManagers != null)
        {
            foreach (var manager in currentSceneManagers.Where(m => m != null))
            {
                try
                {
                    manager.CleanupManager();
                }
                catch (System.NotImplementedException)
                {
                    Debug.LogWarning($"CleanupManager not implemented for {manager.GetType().Name}");
                }
            }
        }
        
        sceneManagersDict.Clear();
        currentSceneManagers = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            
            // Cleanup all managers
            if (coreManagers != null)
            {
                foreach (var manager in coreManagers.Where(m => m != null))
                {
                    try
                    {
                        manager.CleanupManager();
                    }
                    catch (System.NotImplementedException)
                    {
                        Debug.LogWarning($"CleanupManager not implemented for {manager.GetType().Name}");
                    }
                }
            }
            
            if (currentSceneManagers != null)
            {
                foreach (var manager in currentSceneManagers.Where(m => m != null))
                {
                    try
                    {
                        manager.CleanupManager();
                    }
                    catch (System.NotImplementedException)
                    {
                        Debug.LogWarning($"CleanupManager not implemented for {manager.GetType().Name}");
                    }
                }
            }
            
            Instance = null;
            IsInitialized = false;
            sceneManagersDict.Clear();
        }
    }

    // ==== DYNAMIC SCENE MANAGER ACCESS METHODS ====

    /// <summary>
    /// Get a scene manager by type. Returns null if not found.
    /// </summary>
    public static T GetSceneManager<T>() where T : BaseManager
    {
        if (sceneManagersDict.TryGetValue(typeof(T), out BaseManager manager))
        {
            return manager as T;
        }
        return null;
    }

    /// <summary>
    /// Check if a specific scene manager type exists in the current scene
    /// </summary>
    public static bool HasSceneManager<T>() where T : BaseManager
    {
        return sceneManagersDict.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Get all scene managers currently loaded
    /// </summary>
    public static BaseManager[] GetAllSceneManagers()
    {
        return sceneManagersDict.Values.ToArray();
    }

    /// <summary>
    /// Try to get a scene manager, returns true if found
    /// </summary>
    public static bool TryGetSceneManager<T>(out T manager) where T : BaseManager
    {
        manager = GetSceneManager<T>();
        return manager != null;
    }
}