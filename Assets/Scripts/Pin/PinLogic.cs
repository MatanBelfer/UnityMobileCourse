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
    
    public void MovePinToPosition(PinLogic pin, Vector3 worldPosition)
    {
        if (pin == null || pin.gridManager == null) return;

        Debug.Log($"Moving pin {pin.name} to position: {worldPosition}");

        Transform landingPoint = pin.gridManager.GetClosestPoint(worldPosition);
        if (landingPoint != null)
        {
            pin.transform.parent = landingPoint;
            pin.transform.localPosition = Vector3.zero;
            pin.isFollowing = false;

            if (GeometricRubberBand.Instance != null)
            {
                GeometricRubberBand.Instance.UpdateMovingPin(pin.transform,
                    GeometricRubberBand.MovingPinStatus.NotMoving);
            }
        }
    }
    public void StartFollowingPin(PinLogic pin)
    {
        if (pin == null) return;

        pin.isFollowing = true;
        pin.transform.parent = null;

        if (GeometricRubberBand.Instance != null)
        {
            GeometricRubberBand.Instance.UpdateMovingPin(pin.transform, GeometricRubberBand.MovingPinStatus.Moving);
        }
    }
    public void StopFollowingPin(PinLogic pin)
    {
        Debug.Log("inside stop following pin method pin: {" + pin.name + "}");

        if (pin == null || pin.gridManager == null) return;

        Transform landingPoint = pin.gridManager.GetClosestPoint(pin.transform.position);
        if (landingPoint != null)
        {
            pin.transform.parent = landingPoint;
            pin.transform.localPosition = Vector3.zero;
        }

        pin.isFollowing = false;

        if (GeometricRubberBand.Instance != null)
        {
            GeometricRubberBand.Instance.UpdateMovingPin(pin.transform, GeometricRubberBand.MovingPinStatus.NotMoving);
        }
    }
    
}