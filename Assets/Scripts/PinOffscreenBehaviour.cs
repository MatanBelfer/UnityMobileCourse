using UnityEngine;
using System;

public class PinOffscreenBehaviour : MonoBehaviour
{
    public event Action OnWentOffscreen;

    public void Start()
    {
        print("offscreen script");
        OnWentOffscreen += GameManager.Instance.RestartLevel;
    }
    
    public void OnBecameInvisible()
    {
        print($"{gameObject.name} is invisible");
        OnWentOffscreen?.Invoke();
    }
}
