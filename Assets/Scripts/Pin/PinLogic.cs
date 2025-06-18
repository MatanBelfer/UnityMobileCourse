using UnityEngine;
using System.Collections;
using PrimeTween;

public class PinLogic : MonoBehaviour
{
    [SerializeField] private int Row;
    [SerializeField] private int Column;
    [SerializeField] public GridManager gridManager;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;

    public bool isFollowing { get; set; }
    private Tween currentMoveTween;
    
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);

        Transform point = gridManager.GetPointAt(Row, Column);
        if (point != null)
        {
            transform.parent = point;
            transform.localPosition = Vector3.zero;
        }
    }
    
    public void MovePinToPosition(PinLogic pin, Vector3 worldPosition, bool animate = true)
    {
        if (pin == null || pin.gridManager == null) return;

        Transform landingPoint = pin.gridManager.GetClosestPoint(worldPosition);
        if (landingPoint != null)
        {
            if (animate && !pin.isFollowing)
            {
                Vector3 targetWorldPosition = landingPoint.position;
                
                // Notify rubber band that pin is starting to move
                if (GeometricRubberBand.Instance != null)
                {
                    GeometricRubberBand.Instance.UpdateMovingPin(pin.transform, 
                        GeometricRubberBand.MovingPinStatus.Moving);
                }
                
                pin.currentMoveTween = Tween.Position(pin.transform, targetWorldPosition, pin.moveDuration, pin.moveEase)
                    .OnComplete(() =>
                    {
                        // Set parent and local position after animation completes
                        pin.transform.parent = landingPoint;
                        pin.transform.localPosition = Vector3.zero;
                        pin.isFollowing = false;
                        
                        // Notify rubber band that pin stopped moving
                        if (GeometricRubberBand.Instance != null)
                        {
                            GeometricRubberBand.Instance.UpdateMovingPin(pin.transform,
                                GeometricRubberBand.MovingPinStatus.NotMoving);
                        }
                    });
            }
            else
            {
                // Instant movement (for drag mode)
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