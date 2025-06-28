using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder.MeshOperations;

public class GeometricRubberBand : BaseManager, IEnumerable
{
    //calculates the lines composing the rubber band, using anchors and obstacles
    //the calculated area will exclude the obstacles and will be as small as possible, composed of only straight lines 
    //between the anchors and obstacles

    [SerializeField] private Transform[] pins; //the four pins the player moves
    [SerializeField] private Transform[] obstacles; //the obstacles the band cannot move through
                                                    //TODO: get dynamically rom grid manager

    //private List<(Transform,Transform)> connections = new(); //the connections between all transforms that affect the band
    private LinkedList<Transform> activePins; //the pins the band is touching
    private BandVertex movingPin; //the currently moving pin

    private HashSet<BandVertex>
        connectedActivePins =
            new(); //like a linkedlist: each Pin has references to the next and previous Pin, and the next and previous Segments

    //whenever you update this list, you must also spawn or despawn segments accordingly
    //it always remains circular: the last pin connects to the first
    //TODO: use a circular linked list type instead of a HashSet
    private Dictionary<Transform, BandVertex> pinTransformDict = new(); //a dictionary that maps transforms to Pins

    private List<Transform> bandSegments = new(); //the segments of the band - used for animation
    [SerializeField] private string bandSegmentsPool;

    private Vector2 centerOfActivePins; //the center of the active pins
    
    //-------------rework---------------
    /// <summary>
    /// holds a linked structure of Bends that point to segments and anchors.
    /// anchors can move, and when an anchor crosses a segment, it becomes part of the band until it returns
    /// to where it came from 
    /// </summary>
    private Bend rootBend; //the bend from which to start drawing the band
    [Header("Rework")]
    [SerializeField] private RubberBandAnchor[] allAnchors; //all the anchors in the scene
    [SerializeField] private RubberBandAnchor[] initialBand; //the initial set of bends that compose the band
    private LinkedList<Bend> bends = new(); //the bends that compose the band
    private LinkedList<BandSegment> segments = new(); //the segments that compose the band

    protected override void OnInitialize()
    {
        //initialize Pin dictionary
        foreach (Transform pinTrans in pins) pinTransformDict.Add(pinTrans, new BandVertex(pinTrans));
        
        InitializeSegments();
        //
        // //initialize active pins and segments
        // UpdateActivePins();
        // BandVertex firstPin = pinTransformDict[activePins.First.Value];
        // AddFirstConnectedPin(firstPin);
        // if (activePins.Count > 1)
        // {
        //     BandVertex prevPin = firstPin;
        //     LinkedListNode<Transform> newPinNode = activePins.First;
        //     do
        //     {
        //         newPinNode = newPinNode.Next;
        //         BandVertex newPin = pinTransformDict[newPinNode.Value];
        //         AddConnectedPinAfter(prevPin, newPin);
        //         prevPin = newPin;
        //     } while (newPinNode.Next != null);
        // }
        // // LogActivePinNames();
    }

    protected override void OnReset()
    {
        Reset();
    }

    protected override void OnCleanup()
    {
        throw new NotImplementedException();
    }

    // private void Update()
    // {
    //     UpdateBandSegments();
    // }

    private void InitializeSegments()
    {
        //using initialBand, construct the respective Bends
        if (initialBand.Length < 3)
        {
            Debug.Log("Initial band must have at least 3 anchors");
            return;
        }
        
        //first constructs the bends
        for (int i = 0; i < initialBand.Length; i++)
        {
            Bend currentBend;
            RubberBandAnchor anchor = initialBand[i];
            if (i == 0)
            {
                rootBend = new Bend(anchor);
                currentBend = rootBend;
            }
            else
            {
                currentBend = new Bend(anchor);
            }
            bends.AddLast(currentBend);
        }

        //add the segments
        LinkedListNode<Bend> node = bends.Last;
        do
        {
            node = node.NextOrFirst();
            
            BandSegment newSegment = GetSegmentFromPool();
            segments.AddLast(newSegment);
            
            Bend bend = node.Value;
            Bend nextBend = node.NextOrFirst().Value;
            
            bend.nextSegment = newSegment;
            newSegment.prevBend = bend;
            newSegment.nextBend = nextBend;
            nextBend.prevSegment = newSegment;
        } while (!node.IsLast());
    }
    
    private BandSegment GetSegmentFromPool(Bend prevBend)
    {
        BandSegment segment = GetSegmentFromPool();
        segment.prevBend = prevBend;
        return segment;
    }

    private BandSegment GetSegmentFromPool()
    {
        print("GetSegmentFromPool");
        return ManagersLoader.Pool.GetFromPool(bandSegmentsPool).GetComponent<BandSegment>();
    }
    
    private void UpdateActivePins()
    {
        //updates the "activePins" linkedlist and the activity status of the pins

        activePins = PinsOnConvexHull(pins);

        //update the activity status of the pins
        foreach (Transform pinTrans in pins)
        {
            if (pinTransformDict.ContainsKey(pinTrans)) pinTransformDict[pinTrans].active = false;
        }

        foreach (Transform pinTrans in activePins)
        {
            if (pinTransformDict.ContainsKey(pinTrans)) pinTransformDict[pinTrans].active = true;
        }
    }

    private LinkedList<Transform> PinsOnConvexHull(Transform[] points)
    {
        if (points.Length <= 3)
            return points.ToLinkedList();

        LinkedList<Transform> hull = new LinkedList<Transform>();

        // Find the leftmost point (with lowest x-coordinate)
        int leftmostIndex = 0;
        for (int i = 1; i < points.Length; i++)
        {
            if (points[i].position.x < points[leftmostIndex].position.x)
                leftmostIndex = i;
        }

        // Start from leftmost point and keep moving counterclockwise
        int currentPoint = leftmostIndex;
        int nextPoint;

        do
        {
            // Add current point to hull
            hull.AddLast(points[currentPoint]);

            // Find next point with largest counterclockwise angle
            nextPoint = (currentPoint + 1) % points.Length;

            for (int i = 0; i < points.Length; i++)
            {
                if (i == currentPoint) continue;

                // Calculate cross product to determine if points[i] creates a more counterclockwise turn
                Vector2 current = points[currentPoint].position;
                Vector2 next = points[nextPoint].position;
                Vector2 candidate = points[i].position;

                float cross = (next.x - current.x) * (candidate.y - current.y) -
                              (candidate.x - current.x) * (next.y - current.y);

                // If cross product is positive, points[i] creates a more counterclockwise turn
                if (cross > 0 || (cross == 0 &&
                                  Vector2.Distance(current, candidate) > Vector2.Distance(current, next)))
                {
                    nextPoint = i;
                }
            }

            currentPoint = nextPoint;
        } while (currentPoint != leftmostIndex);

        return hull;
    }

    // private void AddFirstConnectedPin(BandVertex newPin)
    // {
    //     if (connectedActivePins.Count != 0)
    //     {
    //         throw new Exception("Cannot add first connected pin when there are already connected pins");
    //     }
    //
    //     connectedActivePins.Add(newPin);
    //     newPin.prevPin = newPin;
    //     newPin.nextPin = newPin;
    //     SetSegmentOfPin(newPin, newPin);
    // }

    // private void SetSegmentOfPin(BandVertex currentPin, BandVertex nextPin)
    // {
    //     if (currentPin.nextSegment == null)
    //     {
    //         var segment = new Segment(bandSegmentsPool, currentPin, nextPin);
    //         bandSegments.Add(segment.transform); // Track new segment
    //     }
    //     else
    //     {
    //         Segment segment = currentPin.nextSegment;
    //         segment.PrevVertex = currentPin;
    //         segment.NextVertex = nextPin;
    //         segment.transform.gameObject.SetActive(true);
    //     }
    // }

    // private void AddConnectedPinAfter(BandVertex refPin, BandVertex newPin)
    // {
    //     BandVertex oldNext = refPin.nextPin; //the pin that was after the reference pin
    //
    //     //remove the old connection
    //     Segment oldSegment = refPin.nextSegment;
    //     // ObjectPoolManager.Instance.InsertToPool(bandSegmentsPool, oldSegment.transform.gameObject);
    //
    //     //add the new pin
    //     connectedActivePins.Add(newPin);
    //
    //     //setup links
    //     refPin.nextPin = newPin;
    //     newPin.prevPin = refPin;
    //     newPin.nextPin = oldNext;
    //     oldNext.prevPin = newPin;
    //
    //     //add segments
    //     SetSegmentOfPin(refPin, newPin);
    //     SetSegmentOfPin(newPin, oldNext);
    // }

    // private void RemoveConnectedPin(BandVertex pinToRemove)
    // {
    //     connectedActivePins.Remove(pinToRemove);
    //     pinToRemove.prevPin.nextPin = pinToRemove.nextPin;
    //     pinToRemove.nextPin.prevPin = pinToRemove.prevPin;
    //
    //     // Return old segment to pool
    //     if (pinToRemove.nextSegment != null)
    //     {
    //         var segmentToRemove = pinToRemove.nextSegment.transform;
    //         bandSegments.Remove(segmentToRemove); // Remove from tracking list
    //         ManagersLoader.Pool.InsertToPool(bandSegmentsPool, segmentToRemove.gameObject);
    //         pinToRemove.nextSegment = null;
    //     }
    //
    //     // Create new segment between prev and next pins
    //     SetSegmentOfPin(pinToRemove.prevPin, pinToRemove.nextPin);
    // }

    private void UpdateBandSegments()
    {
        // //updates the segments of the band if needed (a pin became active)
        //
        // //check if there's a moving pin that will cause the band to update
        // if (movingPin == null) return;
        // UpdateActivePins();
        //
        // //go through activePins and make sure connectedActivePins is in the same order
        // //also track which Pins should be removed (if they're not in activePins)
        // HashSet<BandVertex> pinsToRemove = new();
        // foreach (BandVertex pin in connectedActivePins) pinsToRemove.Add(pin);
        // LinkedListNode<Transform> node = activePins.First;
        // while (node != null)
        // {
        //     //do not remove this pin from connectedActivePins
        //     pinsToRemove.Remove(pinTransformDict[node.Value]);
        //     //check if its next is the same as the corresponding Pin
        //     BandVertex currentPin = pinTransformDict[node.Value];
        //     BandVertex nextPin = pinTransformDict[node.CyclicNext().Value];
        //     if (currentPin.nextPin != nextPin)
        //     {
        //         AddConnectedPinAfter(currentPin, nextPin);
        //         // print("added pin");
        //     }
        //
        //     node = node.Next;
        // }
        //
        // //remove the remaining pins from connectedActivePins
        // foreach (BandVertex pin in pinsToRemove)
        // {
        //     RemoveConnectedPin(pin);
        //     // print("removed pin");
        // }
    }

    public void Reset()
    {
        Debug.Log("Resetting band");
        if (ManagersLoader.Pool == null) Debug.Log("ObjectPoolManager is null");


        Debug.Log($" bandSegments is not null: {bandSegments != null}");

        // Clean up all segments
        foreach (var segment in bandSegments.ToList()) // Use ToList to avoid modification during enumeration
        {
            if (segment != null)
            {
                Debug.Log(
                    $" segment is not null: {segment != null} obj name: {segment.name} , parent is: {segment.parent.gameObject.name}");
                ManagersLoader.Pool.InsertToPool(bandSegmentsPool, segment.gameObject);
            }
        }

        bandSegments.Clear();

        // Reset all pins
        connectedActivePins.Clear();
        foreach (var pin in pinTransformDict.Values)
        {
            // if (pin.nextSegment != null)
            // {
            //     pin.nextSegment = null;
            // }
            //
            // if (pin.prevSegment != null)
            // {
            //     pin.prevSegment = null;
            // }

            pin.nextPin = null;
            pin.prevPin = null;
            pin.active = false;
        }

        movingPin = null;
    }

    // private void LogActivePinNames()
    // {
    //     List<string> activePinsNames = new();
    //     List<string> conActivePinsNames = new();
    //     foreach (Transform pin in activePins)
    //     {
    //         activePinsNames.Add(pin.name);
    //     }
    //
    //     BandVertex currentPin = pinTransformDict[activePins.First.Value];
    //     for (int i = 0; i < connectedActivePins.Count; i++)
    //     {
    //         conActivePinsNames.Add(currentPin.transform.name);
    //         currentPin = currentPin.nextPin;
    //     }
    //
    //     print("activePins: " + string.Join(", ", activePinsNames) + "\n" +
    //           "connectedActivePins: " + string.Join(", ", conActivePinsNames));
    //     ;
    // }

    public void UpdateMovingPin(Transform pin, MovingPinStatus status)
    {
        //update the moving pin
        if (status == MovingPinStatus.Moving)
        {
            movingPin = pinTransformDict[pin];
        }
        else
        {
            movingPin = null;
        }
    }

    public enum MovingPinStatus
    {
        NotMoving,
        Moving
    }
    
    public class Bend
    {
        public RubberBandAnchor anchor { get; private set; }
        public bool isClockwise { get; private set; }
        public BandSegment nextSegment { get; set; }
        public BandSegment prevSegment { get; set; }
        
        public Bend(RubberBandAnchor anchor, bool isClockwise = false)
        {
            this.anchor = anchor;
            this.isClockwise = isClockwise;
        }
    }

    public IEnumerator GetEnumerator()
    {
        return new BandEnumerator(this);
    }

    public class BandEnumerator : IEnumerator
    {
        private GeometricRubberBand band;
        
        public BandEnumerator(GeometricRubberBand band)
        {
            this.band = band;
            Reset();
        }

        private Bend _current;
        public object Current => _current;

        public void Reset()
        {
            _current = band.rootBend;
        }

        public bool MoveNext()
        {
            _current = _current.nextSegment.nextBend;
            return _current != null;
        }
    }
}

public class BandVertex
{
    public Transform transform;
    public bool active; //part of the band
    // public Segment nextSegment; //the segment that follows this pin
    // public Segment prevSegment; //the segment that precedes this pin
    public BandVertex nextPin; //the pin that follows this pin
    public BandVertex prevPin; //the pin that precedes this pin

    public BandVertex(Transform transform)
    {
        this.transform = transform;
    }
}

// public class Segment
// {
//     public Transform transform;
//     private FollowTwoTransforms followScript;
//     private BandVertex prevVertex;
//     private BandVertex nextVertex;
//
//     public BandVertex PrevVertex
//     {
//         get { return prevVertex; }
//         set { followScript.target1 = value.transform; prevVertex = value; }
//     }
//
//     public BandVertex NextVertex
//     {
//         get { return nextVertex; }
//         set { followScript.target2 = value.transform; nextVertex = value;}
//     }
//
//     public Segment(string objectPoolName, BandVertex prevVertex, BandVertex nextVertex)
//     {
//         transform = ManagersLoader.Pool.GetFromPool(objectPoolName).transform;
//         // Parent to GeometricRubberBand immediately after getting from pool
//         transform.SetParent(ManagersLoader.GetSceneManager<GeometricRubberBand>().transform);
//
//         followScript = transform.GetComponent<FollowTwoTransforms>();
//         followScript.target1 = prevVertex.transform;
//         followScript.target2 = nextVertex.transform;
//
//         this.nextVertex = nextVertex;
//         this.prevVertex = prevVertex;
//         this.prevVertex.nextSegment = this;
//         this.nextVertex.prevSegment = this;
//     }
// }