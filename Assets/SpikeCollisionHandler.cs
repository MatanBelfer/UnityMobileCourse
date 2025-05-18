using UnityEngine;

public class Spike : ObjectPoolInterface
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with player or other objects
        if (other.CompareTag("Player"))
        {
            // Add damage or game over logic here
            Debug.Log("Player hit spike!");
        }
    }

    private void OnBecameInvisible()
    {
        // Return to pool when off screen
        if (objectPoolManager != null && gameObject.activeSelf)
        {
            objectPoolManager.InsertToPool(poolName, gameObject);
        }
    }
}