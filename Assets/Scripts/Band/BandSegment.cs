using UnityEngine;

public class BandSegment : MonoBehaviour
{
    [SerializeField] private FollowTwoTransforms followScript;
    
    private GeometricRubberBand.Bend _prevBend;
    public GeometricRubberBand.Bend prevBend
    {
        get => _prevBend;
        set
        {
            _prevBend = value;
            followScript.target1 = value.anchor.transform;
        }
    }

    private GeometricRubberBand.Bend _nextBend;

    public GeometricRubberBand.Bend nextBend
    {
        get => _nextBend;
        set
        {
            _nextBend = value;
            followScript.target2 = value.anchor.transform;
        }
    }
}
