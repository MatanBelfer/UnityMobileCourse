using System;
using UnityEngine;

public class RubberBandAnchor : MonoBehaviour
{
    public Vector2 previousPosition;
    public Vector2 currentPosition;

    private void Start()
    {
        currentPosition = transform.position;
        previousPosition = currentPosition;
    }

    public void Update()
    {
        previousPosition = currentPosition;
        currentPosition = transform.position;
    }
}
