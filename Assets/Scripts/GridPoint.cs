using System;
using UnityEngine;

public class GridPoint :ObjectPoolInterface
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered by {other.gameObject.name}");
        
        ReturnToPool();
        
    }
}
