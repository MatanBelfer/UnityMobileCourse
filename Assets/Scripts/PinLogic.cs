using UnityEngine;
using System.Collections;

public class PinLogic : MonoBehaviour
{
    [SerializeField] private int Row;
    [SerializeField] private int Column;
    [SerializeField] public GridManager gridManager; // Made public so PinInputSystem can access it
    public bool isFollowing { get; set; } // Made setter public so PinInputSystem can control it
    
    private IEnumerator Start()
    {
        // Wait a frame to ensure grid initialization
        yield return new WaitForSeconds(0.1f);

        Transform point = gridManager.GetPointAt(Row, Column);
        if (point != null)
        {
            transform.parent = point;
            transform.localPosition = Vector3.zero;
        }
    }
}