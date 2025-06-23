using UnityEngine;

public class ObjectPoolInterface : MonoBehaviour
{
    // base class for components that interact with the object pool 
    public string poolName;
    
    protected void ReturnToPool()
    {
        if (ManagersLoader.Pool == null)
        {
            // objectPoolManager = ObjectPoolManager.Instance; // Try one more time
            if (ManagersLoader.Pool== null)
            {
                Debug.LogError("ObjectPoolManager is null! Make sure the object is properly initialized through the pool.");
                return;
            }
        }
        
        ManagersLoader.Pool.InsertToPool(poolName, this.gameObject);
    }
    

    
}
