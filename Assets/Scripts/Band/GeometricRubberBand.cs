using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GeometricRubberBand : BaseManager
{
    //calculates the lines composing the rubber band, using anchors and obstacles
    //the calculated area will exclude the obstacles and will be as small as possible, composed of only straight lines 
    //between the anchors and obstacles

    [SerializeField] private Transform[] pins; //the four pins the player moves

    //private List<(Transform,Transform)> connections = new(); //the connections between all transforms that affect the band
    private LinkedList<Transform> activePins; //the pins the band is touching
    private Pin movingPin; //the currently moving pin

    private HashSet<Pin>
        connectedActivePins =
            new(); //like a linkedlist: each Pin has references to the next and previous Pin, and the next and previous Segments

    //whenever you update this list, you must also spawn or despawn segments accordingly
    //it always remains circular: the last pin connects to the first
    //TODO: use a circular linked list type instead of a HashSet
    private Dictionary<Transform, Pin> pinTransformDict = new(); //a dictionary that maps transforms to Pins

    private List<Transform> bandSegments = new(); //the segments of the band - used for animation
    [SerializeField] private string bandSegmentsPool;

    private Vector2 centerOfActivePins; //the center of the active pins
    // [SerializeField] private float pinRadius; //the radius of the pins

    // [SerializeField] private SplineContainer splineContainer;
    // [SerializeField] private float splineTangentLengthRatio;

    // private void Awake()
    // {
    //     InitializeSingleton();
    // }

    protected override void OnInitialize()
    {
        //initialize Pin dictionary
        foreach (Transform pinTrans in pins) pinTransformDict.Add(pinTrans, new Pin(pinTrans));

        //initialize active pins and segments
        UpdateActivePins();
        Pin firstPin = pinTransformDict[activePins.First.Value];
        AddFirstConnectedPin(firstPin);
        if (activePins.Count > 1)
        {
            Pin prevPin = firstPin;
            LinkedListNode<Transform> newPinNode = activePins.First;
            do
            {
                newPinNode = newPinNode.Next;
                Pin newPin = pinTransformDict[newPinNode.Value];
                AddConnectedPinAfter(prevPin, newPin);
                prevPin = newPin;
            } while (newPinNode.Next != null);
        }
        // LogActivePinNames();
    }

    protected override void OnReset()
    {
        Reset();

    }

    protected override void OnCleanup()
    {
        throw new NotImplementedException();
    }

    // private void InitializeSingleton()
    // {
    //     if (Instance != null)
    //     {
    //         Destroy(this);
    //         return;
    //     }
    //
    //     Instance = this;
    // }


    private void Start()
    {
        // //initialize Pin dictionary
        // foreach (Transform pinTrans in pins) pinTransformDict.Add(pinTrans, new Pin(pinTrans));
        //
        // //initialize active pins and segments
        // UpdateActivePins();
        // Pin firstPin = pinTransformDict[activePins.First.Value];
        // AddFirstConnectedPin(firstPin);
        // if (activePins.Count > 1)
        // {
        //     Pin prevPin = firstPin;
        //     LinkedListNode<Transform> newPinNode = activePins.First;
        //     do
        //     {
        //         newPinNode = newPinNode.Next;
        //         Pin newPin = pinTransformDict[newPinNode.Value];
        //         AddConnectedPinAfter(prevPin, newPin);
        //         prevPin = newPin;
        //     } while (newPinNode.Next != null);
        // }
        // // LogActivePinNames();
    }

    private void Update()
    {
        UpdateBandSegments();
        // //debug: number the active pins
        // for (int i = 0; i < activePins.Count; i++)
        // {
        //     Debug.DrawLine(activePins[i].position, activePins[i].position + (i+1) / 2f * Vector3.right, Color.blue, 0.1f);
        // }

        // LinkedListNode<Transform> node = connectedActivePins.First;
        // for (int i = 0; i < connectedActivePins.Count; i++)
        // {
        //     Transform pin = node.Value;
        //     Debug.DrawLine(pin.position, pin.position + (i+1) / 2f * Vector3.up, Color.red, 0.1f);
        //     node = node.Next;
        // }
        //
        // for (int i = 0; i < bandSegments.Count; i++)
        // {
        //     Transform segment = bandSegments[i];
        //     Debug.DrawLine(segment.position, segment.position + (i+1) / 2f * Vector3.left, Color.green, 0.1f);
        // }
    }

    private void UpdateActivePins()
    {
        //updates the "activePins" linkedlist and the activity status of the pins

        activePins = PinsOnConvexHull(pins);

        //debug
        // List<string> msg = new();
        // foreach (Transform pinTrans in activePins)
        // {
        //     msg.Add(pinTrans.name);
        // }
        // print(string.Join(", ", msg));

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
        // Debug.Log($"Starting convex hull calculation with leftmost point: {points[0].name}");


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

    private void AddFirstConnectedPin(Pin newPin)
    {
        if (connectedActivePins.Count != 0)
        {
            throw new Exception("Cannot add first connected pin when there are already connected pins");
        }

        connectedActivePins.Add(newPin);
        newPin.prevPin = newPin;
        newPin.nextPin = newPin;
        SetSegmentOfPin(newPin, newPin);
    }

    private void SetSegmentOfPin(Pin currentPin, Pin nextPin)
    {
        if (currentPin.nextSegment == null)
        {
            var segment = new Segment(bandSegmentsPool, currentPin, nextPin);
            bandSegments.Add(segment.transform); // Track new segment
        }
        else
        {
            Segment segment = currentPin.nextSegment;
            segment.PrevPin = currentPin;
            segment.NextPin = nextPin;
            segment.transform.gameObject.SetActive(true);
        }
    }

    private void AddConnectedPinAfter(Pin refPin, Pin newPin)
    {
        Pin oldNext = refPin.nextPin; //the pin that was after the reference pin

        //remove the old connection
        Segment oldSegment = refPin.nextSegment;
        // ObjectPoolManager.Instance.InsertToPool(bandSegmentsPool, oldSegment.transform.gameObject);

        //add the new pin
        connectedActivePins.Add(newPin);

        //setup links
        refPin.nextPin = newPin;
        newPin.prevPin = refPin;
        newPin.nextPin = oldNext;
        oldNext.prevPin = newPin;

        //add segments
        SetSegmentOfPin(refPin, newPin);
        SetSegmentOfPin(newPin, oldNext);
    }

    private void RemoveConnectedPin(Pin pinToRemove)
    {
        connectedActivePins.Remove(pinToRemove);
        pinToRemove.prevPin.nextPin = pinToRemove.nextPin;
        pinToRemove.nextPin.prevPin = pinToRemove.prevPin;

        // Return old segment to pool
        if (pinToRemove.nextSegment != null)
        {
            var segmentToRemove = pinToRemove.nextSegment.transform;
            bandSegments.Remove(segmentToRemove); // Remove from tracking list
            ManagersLoader.Pool.InsertToPool(bandSegmentsPool, segmentToRemove.gameObject);
            pinToRemove.nextSegment = null;
        }

        // Create new segment between prev and next pins
        SetSegmentOfPin(pinToRemove.prevPin, pinToRemove.nextPin);
    }

    private void UpdateBandSegments()
    {
        //updates the segments of the band if needed (a pin changed its activity status)

        //check if there's a moving pin that will cause the band to update
        if (movingPin == null) return;
        UpdateActivePins();

        //go through activePins and make sure connectedActivePins is in the same order
        //also track which Pins should be removed (if they're not in activePins)
        HashSet<Pin> pinsToRemove = new();
        foreach (Pin pin in connectedActivePins) pinsToRemove.Add(pin);
        LinkedListNode<Transform> node = activePins.First;
        while (node != null)
        {
            //do not remove this pin from connectedActivePins
            pinsToRemove.Remove(pinTransformDict[node.Value]);
            //check if its next is the same as the corresponding Pin
            Pin currentPin = pinTransformDict[node.Value];
            Pin nextPin = pinTransformDict[node.CyclicNext().Value];
            if (currentPin.nextPin != nextPin)
            {
                AddConnectedPinAfter(currentPin, nextPin);
                // print("added pin");
            }

            node = node.Next;
        }

        //remove the remaining pins from connectedActivePins
        foreach (Pin pin in pinsToRemove)
        {
            RemoveConnectedPin(pin);
            // print("removed pin");
        }


        // //check if the pin has changed its activity status from the last frame 
        // bool prevMovingPinIsActive = movingPin.active;
        // UpdateActivePins();
        // bool movingPinIsActive = movingPin.active;
        // if (prevMovingPinIsActive != movingPinIsActive)
        // {
        //     if (movingPinIsActive)
        //     {
        //         //pin became active
        //         //add it to the linked list 
        //         Transform prevPinTrans; LinkedListNode<Transform> node = activePins.First;
        //         while (node.Value != movingPin.transform) node = node.Next;
        //         prevPinTrans = node.CyclicPrevious().Value;
        //         Pin prevPin = pinTransformDict[prevPinTrans];
        //         AddConnectedPinAfter(prevPin, movingPin);
        //     }
        //     else
        //     {
        //         //pin became inactive
        //         //remove it from the linked list
        //         RemoveConnectedPin();
        //     }
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
            if (pin.nextSegment != null)
            {
                pin.nextSegment = null;
            }

            if (pin.prevSegment != null)
            {
                pin.prevSegment = null;
            }

            pin.nextPin = null;
            pin.prevPin = null;
            pin.active = false;
        }

        movingPin = null;
    }

    private void LogActivePinNames()
    {
        List<string> activePinsNames = new();
        List<string> conActivePinsNames = new();
        foreach (Transform pin in activePins)
        {
            activePinsNames.Add(pin.name);
        }

        Pin currentPin = pinTransformDict[activePins.First.Value];
        for (int i = 0; i < connectedActivePins.Count; i++)
        {
            conActivePinsNames.Add(currentPin.transform.name);
            currentPin = currentPin.nextPin;
        }

        print("activePins: " + string.Join(", ", activePinsNames) + "\n" +
              "connectedActivePins: " + string.Join(", ", conActivePinsNames));
        ;
    }

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

    // private void OnDestroy()
    // {
    //     if (Instance == this)
    //     {
    //         Reset();
    //         Instance = null;
    //     }
    // }

    public enum MovingPinStatus
    {
        NotMoving,
        Moving
    }


    // private int NChooseK(int n, int k)
    // {
    //     return Factorial(n) / (Factorial(k) * Factorial(n - k));
    // }

    // private int Factorial(int n)
    // {
    //     return n == 0 ? 1 : n * Factorial(n - 1);
    // }

    // private int PointWithinBounds(Transform pointTransform)
    // {
    //     //if the point is inside the bounds defined by connections, return the index of the closest bound, otherwise return -1
    //     Vector2 point = pointTransform.position;
    //     Vector2 pointOutsideBounds = OuterPoint();
    //     
    //     //count intersections of point<->pointOutsideBounds and the connections 
    //     int totalIntersections = 0;
    //     int indexClosestLine = -1;
    //     float minDist = float.MaxValue;
    //     for (int i = 0; i < connections.Count; i++)
    //     {
    //         //ignore the lines drawn from this point
    //         if (connections[i].Item1 == pointTransform || connections[i].Item2 == pointTransform)
    //         {
    //             continue;
    //         }
    //         
    //         //update minimum
    //         float dist = Mathf.Abs(SignedDistPointLine(point, connections[i].Item1.position, connections[i].Item2.position));
    //         if (dist < minDist)
    //         {
    //             minDist = dist;
    //             indexClosestLine = i;
    //         }
    //
    //         //count intersections
    //         if (DoLinesIntersect(point, pointOutsideBounds, connections[i].Item1.position,
    //                 connections[i].Item2.position))
    //         {
    //             totalIntersections++;
    //         }
    //     }
    //
    //     return totalIntersections % 2 == 1 ? indexClosestLine : -1;
    // }

    private Vector2 OuterPoint()
    {
        return Vector2.right * pins.Max(t => t.position.x) + Vector2.up * pins.Max(t => t.position.y);
    }

    // private bool DoLinesIntersect(Vector2 lineStart1, Vector2 lineEnd1, Vector2 lineStart2, Vector2 lineEnd2)
    // {
    //     return !PointsOnSameSide(lineStart1, lineEnd1, lineStart2, lineEnd2) && !PointsOnSameSide(lineStart2, lineEnd2, lineStart1, lineEnd1);
    // }
    //
    // private bool PointsOnSameSide(Vector2 point1, Vector2 point2, Vector2 lineStart, Vector2 lineEnd)
    // {
    //     return SignedDistPointLine(point1, lineStart, lineEnd) * SignedDistPointLine(point2, lineStart, lineEnd) >= 0;
    // }
    //
    // private float SignedDistPointLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    // {
    //     //returns a signed distance between a point and a line
    //     Vector2 closestPoint = ClosestPointOnLine(point, lineStart, lineEnd, out Vector2 lineDir);
    //     return Vector3.Cross(lineDir, point - closestPoint).z;
    // }
    //
    // private Vector2 ClosestPointOnLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd, out Vector2 lineDir)
    // {
    //     //point on the line defined by lineStart and lineEnd which is closest to point. also outputs the direction of the line in lineDir.
    //     Vector2 lineVec = lineEnd - lineStart;
    //     lineDir = lineVec.normalized;
    //     Vector2 closestPoint = lineStart + Vector2.Dot(point - lineStart, lineDir) * lineDir;
    //     return closestPoint;
    // }
}

public class Pin
{
    public Transform transform;
    public bool active; //part of the band
    public Segment nextSegment; //the segment that follows this pin
    public Segment prevSegment; //the segment that precedes this pin
    public Pin nextPin; //the pin that follows this pin
    public Pin prevPin; //the pin that precedes this pin

    public Pin(Transform transform)
    {
        this.transform = transform;
    }
}

public class Segment
{
    public Transform transform;
    private FollowTwoTransforms followScript;
    private Pin prevPin;
    private Pin nextPin;

    public Pin PrevPin
    {
        get { return prevPin; }
        set { followScript.target1 = value.transform; }
    }

    public Pin NextPin
    {
        get { return nextPin; }
        set { followScript.target2 = value.transform; }
    }

    public Segment(string objectPoolName, Pin prevPin, Pin nextPin)
    {
        transform = ManagersLoader.Pool.GetFromPool(objectPoolName).transform;
        // Parent to GeometricRubberBand immediately after getting from pool
        transform.SetParent(ManagersLoader.GetSceneManager<GeometricRubberBand>().transform);

        followScript = transform.GetComponent<FollowTwoTransforms>();
        followScript.target1 = prevPin.transform;
        followScript.target2 = nextPin.transform;

        this.nextPin = nextPin;
        this.prevPin = prevPin;
        this.prevPin.nextSegment = this;
        this.nextPin.prevSegment = this;
    }
}