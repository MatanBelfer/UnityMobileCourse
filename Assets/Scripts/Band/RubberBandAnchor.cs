using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RubberBandAnchor : MonoBehaviour
{
    public Vector2 previousPosition;
    public Vector2 currentPosition;
    [SerializeField] private float _baseRadius; //a segment touching this anchor should be displaced by this radius
    public float baseRadius => _baseRadius;
    
    public HashSet<GeometricRubberBand.Bend> bendsOnMe = new();

    public void UpdatePosition()
    {
        previousPosition = currentPosition;
        currentPosition = transform.position;
    }

    public int GetNumWraps(Vector2 direction)
    {
        return bendsOnMe.Count(
            bend => direction.IsBetween(
                bend.prevSegment.leftRotated,bend.nextSegment.leftRotated, bend.isClockwise));
    }
}
