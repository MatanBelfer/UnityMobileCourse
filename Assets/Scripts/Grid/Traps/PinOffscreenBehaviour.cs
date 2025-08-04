using UnityEngine;
using System;

public class PinOffscreenBehaviour : MonoBehaviour
{
    public event Action OnWentOffscreen;

    public void Start()
    {
        print("offscreen script");
        OnWentOffscreen += ManagersLoader.Game.RestartLevel;
        //make sure this script turns off before the game restarts, otherwise it triggers unwantedly
        ManagersLoader.Game.OnRestartLevel += () => this.enabled = false;
    }
    
    public void OnBecameInvisible()
    {
        print($"{gameObject.name} is invisible");
        OnWentOffscreen?.Invoke();
    }
}
