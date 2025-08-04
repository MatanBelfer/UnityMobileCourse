using UnityEngine;

public class DownBorder : MonoBehaviour
{
    private int pointsInCurrentRow = 0;
    private int pointsNeededForRow = 0;
    

    private void OnTriggerEnter(Collider other)
    {
        var poolInterface = other.GetComponent<ObjectPoolInterface>();
        if (poolInterface != null)
        {
            ManagersLoader.GetSceneManager<GridManager>().ReturnObjectToPool(other.gameObject);
            pointsInCurrentRow++;

            pointsNeededForRow = ManagersLoader.GetSceneManager<GridManager>().GetCurrentRowPointCount();

            if (pointsInCurrentRow >= pointsNeededForRow)
            {
                ManagersLoader.GetSceneManager<GridManager>().SpawnNewRow();
                pointsInCurrentRow = 0;
            }
        }
    }
}