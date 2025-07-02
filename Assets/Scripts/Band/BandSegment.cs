using System;
using UnityEngine;
using Unity.Mathematics;

public class BandSegment : MonoBehaviour
{
    [SerializeField] private FollowTwoTransforms followScript;

    private void Awake()
    {
        followScript.follow = false;
    }

    private GeometricRubberBand.Bend _prevBend;
    private Transform prevTransform;
    public GeometricRubberBand.Bend prevBend
    {
        get => _prevBend;
        set
        {
            _prevBend = value;
            // prevTransform = value.anchor.transform;
            // followScript.target1 = prevTransform;
        }
    }

    private GeometricRubberBand.Bend _nextBend;
    private Transform nextTransform;
    public GeometricRubberBand.Bend nextBend
    {
        get => _nextBend;
        set
        {
            _nextBend = value;
            // nextTransform = value.anchor.transform;
            // followScript.target2 = nextTransform;
        }
    }


    public Vector2[] currentPos { get; private set; } = new Vector2[2]; /// <summary>
                                                                       /// the first element is the startPos, the second element is endPos
                                                                       /// </summary>

    public Vector2[] previousPos { get; private set; } = new Vector2[2];

    public Vector2 startPos
    {
        get => currentPos[0];
        // private set
        // {
        //     currentPos[0] = value;
        //     followScript.UpdatePosition(value, currentPos[1]);
        // }
    }
    
    public Vector2 endPos
    {
        get => currentPos[1];
        // private set
        // {
        //     currentPos[1] = value;
        //     followScript.UpdatePosition(currentPos[0], value);
        // }
    }

    // public void Start()
    // {
    //     UpdatePosition();
    // }
    
    public void UpdatePosition(Vector2 startPos, Vector2 endPos)
    {
        for (int i = 0; i < 2; i++)
        {
            previousPos[i] = currentPos[i];
        }
        currentPos[0] = startPos;
        currentPos[1] = endPos;
        followScript.UpdatePosition(startPos, endPos);
    }
}
