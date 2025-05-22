using UnityEngine;
using System.Collections;

public class PinLogic : MonoBehaviour
{
    [SerializeField] private int Row;
    [SerializeField] private int Column;
    [SerializeField] private GridManager gridManager;
    
    public bool isFollowing { get; private set; }
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
    

    private void Update()
    {
        if (isFollowing)
        {
            Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousepos.z = 0f;
            transform.position = mousepos;
        }
    }

    private void OnMouseDown()
    {
        if (!isFollowing)
        {
            StartFollowing();
        }
    }

    private void OnMouseUp()
    {
        if (isFollowing)
        {
            StopFollowing();
        }
    }

    public void StartFollowing()
    {
        isFollowing = true;
        transform.parent = null;
        GeometricRubberBand.Instance.UpdateMovingPin(transform, GeometricRubberBand.MovingPinStatus.Moving);
    }

    public void StopFollowing()
    {
        //find the closest point on the grid and go to it
        Transform landingPoint = gridManager.GetClosestPoint(transform.position);
        transform.parent = landingPoint;
        transform.localPosition = Vector3.zero;

        isFollowing = false;
        //update the band that it's not longer moving
        GeometricRubberBand.Instance.UpdateMovingPin(transform, GeometricRubberBand.MovingPinStatus.NotMoving);
    }
}