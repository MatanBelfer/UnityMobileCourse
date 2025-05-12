using UnityEngine;

public class DownBorder : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    private ObjectPoolManager objectPoolManager;
    private int pointsInCurrentRow = 0;
    private int pointsNeededForRow = 0;

    private void Start()
    {
        objectPoolManager = ObjectPoolManager.Instance;
        if (objectPoolManager == null)
        {
            Debug.LogError("ObjectPoolManager not found!");
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var poolInterface = other.GetComponent<ObjectPoolInterface>();
        if (poolInterface != null)
        {
            // Return the point to the pool
            objectPoolManager.InsertToPool(poolInterface.poolName, other.gameObject);
            
            // Increment points counter and check if row is complete
            pointsInCurrentRow++;
            
            // Get points needed for current row (alternating between n and n-1)
            pointsNeededForRow = gridManager.GetCurrentRowPointCount();
            if (pointsInCurrentRow >= pointsNeededForRow)
            {
                gridManager.SpawnNewRow();
                pointsInCurrentRow = 0;
            }
        }
    }
}