using System;
using UnityEngine;
using System.Collections;
using PrimeTween;

public class PinLogic : MonoBehaviour
{
    [SerializeField] private int Row;
    [SerializeField] private int Column;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;

    public bool isFollowing { get; set; }
    private Tween currentMoveTween;
    private GridPoint _currentGridPoint;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);

        GridPoint point = ManagersLoader.GetSceneManager<GridManager>().GetPointAt(Row, Column);
        if (point != null)
        {
            transform.parent = point.transform;
            transform.localPosition = Vector3.zero;

            // Set the current grid point reference
            _currentGridPoint = point.GetComponent<GridPoint>();
            if (_currentGridPoint != null)
            {
                _currentGridPoint.isBlocked = true; // Mark as blocked since pin is occupying it
            }

            ManagersLoader.Game?.SetInitialScore(Row);
        }
    }

    public void MovePinToPosition(Vector3 worldPosition, bool animate = true)
    {
        GridPoint landingPoint = ManagersLoader.GetSceneManager<GridManager>()
            .GetClosestPoint(worldPosition, out int chosenRow);

        GridPoint _gridLandingPoint = landingPoint.GetComponent<GridPoint>();

//        Debug.Log($"Target world position: {worldPosition}");
//        Debug.Log($"Closest point found: {landingPoint.name} at position: {landingPoint.position}");
//        Debug.Log($"Is closest point blocked: {_gridLandingPoint.isBlocked}");

        if (_gridLandingPoint == _currentGridPoint)
        {
            Debug.Log("Pin is already on the same point, skipping move");
            return;
        }
        
        if (landingPoint != null)
        {
            if (_gridLandingPoint.isBlocked)
            {
                Debug.Log("Initial point is blocked, searching for next closest available point");

                GridManager gridManager = ManagersLoader.GetSceneManager<GridManager>();
                GridPoint nextClosestPoint = gridManager.GetClosestAvailablePoint(worldPosition, out int newChosenRow);

                if (nextClosestPoint != null)
                {
                    landingPoint = nextClosestPoint;
                    _gridLandingPoint = landingPoint.GetComponent<GridPoint>();
                    chosenRow = newChosenRow;
                    Debug.Log(
                        $"Found alternative point: {landingPoint.name} at position: {landingPoint.transform.position} at row {chosenRow}");
                }
                else
                {
                    Debug.LogWarning("No available points found for pin placement");
                    return;
                }
            }
            else
            {
                Debug.Log($"Using original point: {landingPoint.name} at position: {landingPoint.transform.position}");
            }

            if (animate)
            {
                print($"{gameObject.name} is moving to {landingPoint.transform.position}");
                Vector3 targetWorldPosition = landingPoint.transform.position;

                // Notify rubber band that pin is starting to move
                if (ManagersLoader.GetSceneManager<GeometricRubberBand>() != null)
                {
                    ManagersLoader.GetSceneManager<GeometricRubberBand>().UpdateMovingPin(transform,
                        GeometricRubberBand.MovingPinStatus.Moving);
                }

                currentMoveTween = Tween
                    .Position(transform, targetWorldPosition, moveDuration, moveEase)
                    .OnComplete(() =>
                    {
                        transform.parent = landingPoint.transform;
                        transform.localPosition = Vector3.zero;
                        _currentGridPoint = _gridLandingPoint;
                        _currentGridPoint.isBlocked = true;
                        isFollowing = false;

                        Debug.Log($"Pin landed at: {transform.position} (parent: {landingPoint.name})");

                        if (ManagersLoader.GetSceneManager<GeometricRubberBand>() != null)
                        {
                            ManagersLoader.GetSceneManager<GeometricRubberBand>().UpdateMovingPin(transform,
                                GeometricRubberBand.MovingPinStatus.NotMoving);
                        }
                    });
            }
            else
            {
                // Instant movement (for drag mode)
                transform.parent = landingPoint.transform;
                transform.localPosition = Vector3.zero;
                _currentGridPoint = _gridLandingPoint;
                // _currentGridPoint.isBlocked = true;
                isFollowing = false;

                Debug.Log($"Pin instantly moved to: {transform.position} (parent: {landingPoint.name})");

                if (ManagersLoader.GetSceneManager<GeometricRubberBand>() != null)
                {
                    ManagersLoader.GetSceneManager<GeometricRubberBand>().UpdateMovingPin(transform,
                        GeometricRubberBand.MovingPinStatus.NotMoving);
                }
            }

            ManagersLoader.Game?.UpdateScore(chosenRow);
        }
    }


    public void StartFollowingPin()
    {
        Debug.Log($" pin current grid point isnull: {_currentGridPoint == null}");

        // Only try to access _currentGridPoint if it's not null
        if (_currentGridPoint != null)
        {
            Debug.Log($"is point blocked? {_currentGridPoint.isBlocked}");
            _currentGridPoint.isBlocked = false; // Free up the current position
            Debug.Log($"is point blocked? {_currentGridPoint.isBlocked}");

            _currentGridPoint = null; // Clear the reference
        }
        else
        {
            Debug.Log("Current grid point is null - pin may not be placed on grid yet");
        }

        isFollowing = true;
        transform.parent = null;

        if (ManagersLoader.GetSceneManager<GeometricRubberBand>() != null)
        {
            ManagersLoader.GetSceneManager<GeometricRubberBand>()
                .UpdateMovingPin(transform, GeometricRubberBand.MovingPinStatus.Moving);
        }
    }

    public void StopFollowingPin()
    {
//        Debug.Log("inside stop following pin method pin: {" + name + "}");


        GridPoint landingPoint = ManagersLoader.GetSceneManager<GridManager>().GetClosestPoint(transform.position);
        if (landingPoint != null )
        {
            transform.parent = landingPoint.transform;
            transform.localPosition = Vector3.zero;
            _currentGridPoint.isBlocked = true;
        }
        else
        {
            landingPoint = FindNextClosestAvailablePoint(transform.position);
            transform.parent = landingPoint.transform;
            transform.localPosition = Vector3.zero;
            landingPoint.GetComponent<GridPoint>().isBlocked = true;
        }

        _currentGridPoint = landingPoint.GetComponent<GridPoint>();
        isFollowing = false;

        if (ManagersLoader.GetSceneManager<GeometricRubberBand>() != null)
        {
            ManagersLoader.GetSceneManager<GeometricRubberBand>()
                .UpdateMovingPin(transform, GeometricRubberBand.MovingPinStatus.NotMoving);
        }
    }

    private GridPoint FindNextClosestAvailablePoint(Vector3 worldPosition)
    {
        GridManager gridManager = ManagersLoader.GetSceneManager<GridManager>();

        // Use the new GridManager method to find the closest available point
        return gridManager.GetClosestAvailablePoint(worldPosition, out int _);
    }
}