using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SpikeInteraction : MonoBehaviour
{
    public event Action OnTouchSpike;

    public void Start()
    {
        OnTouchSpike += ManagersLoader.Game.HitBySpike;
        OnTouchSpike += () => ManagersLoader.Audio.PlaySFX("band_snap");
    }
    
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Spike"))
        {
            // print("Restarting scene");
            
            // Use the new dynamic scene manager access
            var rubberBand = ManagersLoader.GetSceneManager<GeometricRubberBand>();
            if (rubberBand != null)
            {
                rubberBand.Reset();  
            }
            
            // Clean up grid manager
            if (ManagersLoader.GetSceneManager<GridManager>() != null)
            {
                Destroy(ManagersLoader.GetSceneManager<GridManager>().gameObject);
            }
            
            // Clean up rubber band if it still exists
            if (rubberBand != null)
            {
                Destroy(rubberBand.gameObject);
            }

            OnTouchSpike?.Invoke();
            print("Invoked OnTouchSpike");

        }
    }
}