using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SpikeInteraction : MonoBehaviour
{
    public event Action OnTouchSpike;

    public void Start()
    {
        OnTouchSpike += GameManager.Instance.RestartLevel;
    }
    
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Spike"))
        {
            print("Restarting scene");
            
            if (GeometricRubberBand.Instance != null)
            {
                print(1);
                GeometricRubberBand.Instance.Reset();  
            }
            
            if (GridManager.Instance != null)
            {
                Destroy(GridManager.Instance.gameObject);
            }
            
            if (GeometricRubberBand.Instance != null)
            {
                Destroy(GeometricRubberBand.Instance.gameObject);
            }

            OnTouchSpike?.Invoke();
        }
    }
}