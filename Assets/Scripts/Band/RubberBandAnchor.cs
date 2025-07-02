using System;
using UnityEngine;

public class RubberBandAnchor : MonoBehaviour
{
    public Vector2 previousPosition;
    public Vector2 currentPosition;
    [SerializeField] public float baseRadius; //a segment touching this anchor should be displaced by this radius

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
