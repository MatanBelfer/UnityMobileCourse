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
            gridManager.RemovePoint(other.transform);

            objectPoolManager.InsertToPool(poolInterface.poolName, other.gameObject);

            pointsInCurrentRow++;

            pointsNeededForRow = gridManager.GetCurrentRowPointCount();

            if (pointsInCurrentRow >= pointsNeededForRow)
            {
                gridManager.SpawnNewRow();
                pointsInCurrentRow = 0;
            }
        }
    }
}