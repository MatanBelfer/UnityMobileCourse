using System;
using UnityEngine;

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
            prevTransform = value.anchor.transform;
            followScript.target1 = prevTransform;
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
            nextTransform = value.anchor.transform;
            followScript.target2 = nextTransform;
        }
    }
    
    
    private Vector2[] _position = new Vector2[2];
    public Vector2 position1
    {
        get => _position[0];
        set
        {
            _position[0] = value;
            followScript.UpdatePosition(value, _position[1]);
        }
    }public Vector2 position2
    {
        get => _position[1];
        set
        {
            _position[1] = value;
            followScript.UpdatePosition(_position[0], value);
        }
    }
    
    // private Vector2[] previousPosition { get; set; } = new Vector2[2];

//     private void Start()
//     {
//         followScript.OnMove += RecordPositions;
//         
//         currentPosition[0] = prevTransform.position;
//         currentPosition[1] = nextTransform.position;
//         for (int i = 0; i < 2; i++)
//         {
//             previousPosition[i] = currentPosition[i];
//         }
//     }
//
//     private void Update()
//     {
//         RecordPositions();
//     }
//
//     private void RecordPositions()
//     {
//         for (int i = 0; i < 2; i++)
//         {
//             previousPosition[i] = currentPosition[i];
//         }
//         currentPosition[0] = prevTransform.position;
//         currentPosition[1] = nextTransform.position;
//     }
}
