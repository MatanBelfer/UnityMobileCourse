
public class Spike : ObjectPoolInterface
{
    private void OnBecameInvisible()
    {
        // Return to pool 
        if (objectPoolManager != null && gameObject.activeSelf)
        {
            objectPoolManager.InsertToPool(poolName, gameObject);
        }
    }
}