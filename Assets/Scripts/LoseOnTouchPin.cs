using System;
using UnityEngine;

public class LoseOnTouchPin : MonoBehaviour
{
    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.CompareTag("Pin"))
        {
            gameManager.RestartLevel();
        }
    }
}
