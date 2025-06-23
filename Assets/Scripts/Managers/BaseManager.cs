using UnityEngine;

public abstract class BaseManager : MonoBehaviour
{
    public bool IsInitialized { get; protected set; }

    protected virtual void Awake()
    {
    }

    public virtual void InitManager()
    {
        if (IsInitialized) return;

        Debug.Log($"Initializing {GetType().Name}");
        OnInitialize();
        IsInitialized = true;
    }

    public virtual void ResetManager()
    {
        Debug.Log($"Resetting {GetType().Name}");
        OnReset();
    }

    public virtual void CleanupManager()
    {
        Debug.Log($"Cleaning up {GetType().Name}");
        OnCleanup();
        IsInitialized = false;
    }

    // Override these in derived classes
    protected abstract void OnInitialize();
    protected abstract void OnReset();
    protected abstract void OnCleanup();
}