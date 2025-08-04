using UnityEngine;
using PrimeTween;

public class Collectable : ObjectPoolInterface
{
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private float collectAnimation = 0.3f;
    [SerializeField] private GameObject collectEffect; // Optional particle effect

    private bool isCollected = false;
    private Tween floatingTween;

    private void Start()
    {
        // Add a gentle floating animation to make it more appealing
        floatingTween = Tween.PositionY(transform, transform.position.y + 0.2f, 1f, Ease.InOutSine, -1, CycleMode.Yoyo);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        // Check if it's a pin that collected this
        PinLogic pin = other.GetComponent<PinLogic>();
        if (pin != null)
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        isCollected = true;

        // Stop floating animation
        if (floatingTween.isAlive)
        {
            floatingTween.Stop();
        }

        // Add score to game manager
        if (ManagersLoader.Game != null)
        {
            ManagersLoader.Game.AddScore(scoreValue);
        }

        // Play collect effect if available
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Animate collection and return to pool
        Tween.Scale(transform, Vector3.zero, collectAnimation, Ease.InBack)
            .OnComplete(() => { ReturnToPool(); });
    }

    private void ReturnToPool()
    {
        isCollected = false;
        transform.localScale = Vector3.one;

        // Return to object pool
        ObjectPoolManager poolManager = ManagersLoader.GetSceneManager<ObjectPoolManager>();
        if (poolManager != null)
        {
            GridParameters gridParams = ManagersLoader.GetSceneManager<GridManager>().gridParameters;
            poolManager.ReturnToPool(gridParams.collectablePoolName, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void SetScoreValue(int value)
    {
        scoreValue = value;
    }

    // Reset collectable when spawned from pool
    public void ResetCollectable()
    {
        isCollected = false;
        transform.localScale = Vector3.one;

        // Restart floating animation
        if (gameObject.activeInHierarchy)
        {
            floatingTween = Tween.PositionY(transform, transform.position.y + 0.2f, 1f, Ease.InOutSine, -1,
                CycleMode.Yoyo);
        }
    }

    private void OnDisable()
    {
        // Clean up tween when object is disabled
        if (floatingTween.isAlive)
        {
            floatingTween.Stop();
        }
    }
}