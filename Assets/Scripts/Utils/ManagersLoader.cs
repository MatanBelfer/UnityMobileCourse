using UnityEngine;

public class ManagersLoader : MonoBehaviour
{
    [SerializeField] private ObjectPoolManager poolManager;
    public static bool IsInitialized { get; private set; }
    public static ManagersLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (poolManager == null)
        {
            Debug.LogError("ObjectPoolManager reference not set in ManagersLoader!");
            return;
        }

        poolManager.gameObject.SetActive(true);
        DontDestroyOnLoad(poolManager.gameObject);

        // Wait a frame to ensure ObjectPoolManager's Awake has run
        StartCoroutine(InitializeAfterPoolManager());
    }

    private System.Collections.IEnumerator InitializeAfterPoolManager()
    {
        yield return new WaitForEndOfFrame();

        if (ObjectPoolManager.Instance != null)
        {
            IsInitialized = true;
        }
        else
        {
            Debug.LogError("ObjectPoolManager failed to initialize properly!");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            IsInitialized = false;
        }
    }
}