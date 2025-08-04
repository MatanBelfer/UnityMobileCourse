using UnityEngine;
using PrimeTween;

public class MovingSpikeTrap : ObjectPoolInterface
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float moveRange = 3f;
    
    private Vector3 startPosition;
    private Vector3 leftBound;
    private Vector3 rightBound;
    private int direction = 1; // 1 for right, -1 for left
    private bool isMoving = false;
    private Transform parentGridPoint;
    
    private void Start()
    {
        InitializeMovement();
    }
    
    private void InitializeMovement()
    {
        // Store reference to parent grid point for moving down with grid
        parentGridPoint = transform.parent;
        
        startPosition = transform.localPosition; // Use local position relative to grid point
        leftBound = startPosition + Vector3.left * (moveRange / 2f);
        rightBound = startPosition + Vector3.right * (moveRange / 2f);
        isMoving = true;
    }
    
    private void Update()
    {
        if (!isMoving) return;
        
        // Move horizontally using local position (so it moves with the grid)
        Vector3 localPos = transform.localPosition;
        localPos.x += direction * moveSpeed * Time.deltaTime;
        transform.localPosition = localPos;
        
        // Check bounds and reverse direction
        if (direction > 0 && transform.localPosition.x >= rightBound.x)
        {
            direction = -1;
            transform.localPosition = new Vector3(rightBound.x, transform.localPosition.y, transform.localPosition.z);
        }
        else if (direction < 0 && transform.localPosition.x <= leftBound.x)
        {
            direction = 1;
            transform.localPosition = new Vector3(leftBound.x, transform.localPosition.y, transform.localPosition.z);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if it hit a pin
        PinLogic pin = other.GetComponent<PinLogic>();
        if (pin != null)
        {
            // Trigger lose condition
            if (ManagersLoader.Game != null)
            {
                ManagersLoader.Game.HitBySpike();
            }
        }
    }
    
    public void SetMovementParameters(float speed, float range)
    {
        moveSpeed = speed;
        moveRange = range;
        
        // Recalculate bounds if already initialized
        if (startPosition != Vector3.zero)
        {
            leftBound = startPosition + Vector3.left * (moveRange / 2f);
            rightBound = startPosition + Vector3.right * (moveRange / 2f);
        }
    }
    
    public void StopMovement()
    {
        isMoving = false;
    }
    
    public void ResumeMovement()
    {
        isMoving = true;
    }
    
    // Reset spike when spawned from pool
    public void ResetSpike()
    {
        direction = 1;
        isMoving = false;
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
    }
    
    private void ReturnToPool()
    {
        ObjectPoolManager poolManager = ManagersLoader.GetSceneManager<ObjectPoolManager>();
        if (poolManager != null)
        {
            GridParameters gridParams = ManagersLoader.GetSceneManager<GridManager>().gridParameters;
            poolManager.ReturnToPool(gridParams.collectablePoolName, gameObject);        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}