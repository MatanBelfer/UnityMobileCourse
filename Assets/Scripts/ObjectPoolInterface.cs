using UnityEngine;
using UnityEngine.Rendering;

public class ObjectPoolInterface : MonoBehaviour
{
    // base class for components that interact with the object pool 
    public string poolName;
    public ObjectPoolManager objectPoolManager;

    protected void ReturnToPool()
    {
        objectPoolManager.InsertToPool(poolName, this.gameObject);
    }
    
}
