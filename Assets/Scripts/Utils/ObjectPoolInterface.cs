using UnityEngine;

public class ObjectPoolInterface : MonoBehaviour
{
    // base class for components that interact with the object pool 
    public string poolName;
    public ObjectPoolManager objectPoolManager;

    protected virtual void Awake()
    {
        if (objectPoolManager == null)
        {
            objectPoolManager = ObjectPoolManager.Instance;
        }
    }

    protected void ReturnToPool()
    {
        if (objectPoolManager == null)
        {
            objectPoolManager = ObjectPoolManager.Instance; // Try one more time
            if (objectPoolManager == null)
            {
                Debug.LogError("ObjectPoolManager is null! Make sure the object is properly initialized through the pool.");
                return;
            }
        }
        
        objectPoolManager.InsertToPool(poolName, this.gameObject);
    }
    

    
}
