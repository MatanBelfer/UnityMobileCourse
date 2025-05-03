using System;
using UnityEngine;

public class DespawnOnClick : ObjectPoolInterface
{
    private void OnMouseDown()
    {
        ReturnToPool();
    }
}
