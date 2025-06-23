using System;
using UnityEngine;

public class LoseOnTouchPin : MonoBehaviour
{
    // GameManager gameManager;

    // void Start()
    // {
    //     gameManager = GameManager.Instance;
    // }

    private void OnTriggerEnter(Collider other)
    {
        Transform parent = other.transform?.parent;
        if (!parent) return;
        
        if (parent.CompareTag("Pin"))
        {
            ManagersLoader.Game.RestartLevel();
        }
    }
}
