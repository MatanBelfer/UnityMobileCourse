public class Spike : ObjectPoolInterface
{
    private void OnBecameInvisible()
    {
        if (!gameObject.scene.isLoaded)
            return;
            
        // If grid is being deactivated (during scene transitions), just destroy the spike
        if (transform.parent != null && !transform.parent.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }
        
        // Normal pooling during gameplay
        objectPoolManager.InsertToPool(poolName, gameObject);
    }
}