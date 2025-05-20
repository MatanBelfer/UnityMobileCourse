using UnityEngine;

public class ManagersLoader : MonoBehaviour
{
    [SerializeField] private ObjectPoolManager poolManager;
    public static bool IsInitialized { get; private set; }
    
    private void Awake()
    {
        if (poolManager == null)
        {
            Debug.LogError("ObjectPoolManager reference not set in ManagersLoader!");
            return;
        }

        // Set up the pool manager first and make sure it's initialized
        poolManager.gameObject.SetActive(true);
        
        // Wait a frame to ensure ObjectPoolManager's Awake has run
        StartCoroutine(InitializeAfterPoolManager());
    }

    private System.Collections.IEnumerator InitializeAfterPoolManager()
    {
        yield return new WaitForEndOfFrame(); // Wait one frame
        
        if (ObjectPoolManager.Instance != null)
        {
            // DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(poolManager);
            IsInitialized = true;
        }
        else
        {
            Debug.LogError("ObjectPoolManager failed to initialize properly!");
        }
    }

}