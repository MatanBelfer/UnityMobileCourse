using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using System;

public class PinLogic : MonoBehaviour
{
    [FormerlySerializedAs("Row")] [SerializeField] private int StartingRow; // zero based
    [FormerlySerializedAs("Column")] [SerializeField] private int StartingColumn; // zero based
    [SerializeField] private GridManager gridManager;
    private GameManager gameManager;
    private int currentRow;
    public event Action<int> OnStopFollowing;
    // [SerializeField] private PinOffscreenBehaviour offscreenBehaviour;
    
    public bool isFollowing { get; private set; }

    private IEnumerator Start()
    {
        // Wait a frame to ensure grid initialization
        yield return new WaitForSeconds(0.1f);

        Transform point = gridManager.GetPointAt(StartingRow, StartingColumn);
        if (point != null)
        {
            transform.parent = point;
            transform.localPosition = Vector3.zero;

            // //turn on the behaviour that loses the game when the pin goes off-screen
            // offscreenBehaviour.enabled = true;
        }

        gameManager = GameManager.Instance;
        OnStopFollowing += r => { if (r != -1) gameManager.UpdateScore(r); };
        gameManager.SetInitialScore(StartingRow + 1);
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
        Transform landingPoint = gridManager.GetClosestPoint(transform.position, out currentRow);
        transform.parent = landingPoint;
        transform.localPosition = Vector3.zero;
        //update the score on the game manager
        // print($"{gameObject.name} landed on row {currentRow}");
        OnStopFollowing?.Invoke(currentRow);

        isFollowing = false;
        //update the band that it's not longer moving
        GeometricRubberBand.Instance.UpdateMovingPin(transform, GeometricRubberBand.MovingPinStatus.NotMoving);
    }
}