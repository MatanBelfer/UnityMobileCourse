using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GeometricRubberBand : MonoBehaviour
{
    //calculates the lines composing the rubber band, using anchors and obstacles
    //the calculated area will exclude the obstacles and will be as small as possible, composed of only straight lines 
    //between the anchors and obstacles
    
    [SerializeField] private Transform[] pins; //the four pins the player moves
    //private List<(Transform,Transform)> connections = new(); //the connections between all transforms that affect the band
    private LinkedList<Transform> activePins; //the pins the band is touching
    private Pin movingPin; //the currently moving pin
    private HashSet<Pin> connectedActivePins = new(); //like a linkedlist: each Pin has references to the next and previous Pin, and the next and previous Segments
    //whenever you update this list, you must also spawn or despawn segments accordingly
    //it always remains circular: the last pin connects to the first
    private Dictionary<Transform, Pin> pinTransformDict = new(); //a dictionary that maps transforms to Pins
    
    private List<Transform> bandSegments = new(); //the segments of the band - used for animation
    [SerializeField] private string bandSegmentsPool;
    
    private Vector2 centerOfActivePins; //the center of the active pins
    // [SerializeField] private float pinRadius; //the radius of the pins
    
    // [SerializeField] private SplineContainer splineContainer;
    // [SerializeField] private float splineTangentLengthRatio;
    
    public static GeometricRubberBand Instance { get; private set; }

    private void Awake()
    {
        InitializeSingleton();
    }

    private void InitializeSingleton()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
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

    // private void CalculateActivePins()
    // {
    //     //find pins that define the shape of the band
    //     activePins = pins.Where(p => PointWithinBounds(p) == -1).ToArray();
    //     
    //     //find center of active pins
    //     centerOfActivePins = activePins.Select(p => p.position).
    //         Aggregate((a, b) => a + b) / pins.Length;
    //     
    //     //sort active pins counterclockwise
    //     activePins = activePins.OrderBy(_ => _, new PinComparer(centerOfActivePins)).ToArray();
    // }

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

    private void AddFirstConnectedPin(Pin newPin)
    {
        if (connectedActivePins.Count != 0)
        {
            throw new Exception("Cannot add first connected pin when there are already connected pins");
        }

        connectedActivePins.Add(newPin);
        newPin.prevPin = newPin;
        newPin.nextPin = newPin;
        new Segment(bandSegmentsPool, newPin, newPin);
    }
    
    private void AddConnectedPinAfter(Pin refPin, Pin newPin)
    {
        Pin oldNext = refPin.nextPin; //the pin that was after the reference pin
        
        //remove the old connection
        Segment oldSegment = refPin.nextSegment;
        ObjectPoolManager.Instance.InsertToPool(bandSegmentsPool, oldSegment.transform.gameObject);
        
        //add the new pin
        connectedActivePins.Add(newPin);
        
        //setup connections
        refPin.nextPin = newPin;
        newPin.prevPin = refPin;
        newPin.nextPin = oldNext;
        oldNext.prevPin = newPin;
        
        //add segments
        new Segment(bandSegmentsPool, refPin, newPin);
        new Segment(bandSegmentsPool, newPin, oldNext);

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

    private void UpdateBandSegments()
    {
        //updates the segments of the band if needed (a pin changed its activity status)
        
        //check if there's a moving pin that will cause the band to update
        if (movingPin == null) return;
        
        //check if the pin has changed its activity status from the last frame 
        bool prevMovingPinIsActive = movingPin.active;
        UpdateActivePins();
        bool movingPinIsActive = movingPin.active;
        if (prevMovingPinIsActive == movingPinIsActive) return;
        
        if (movingPinIsActive)
        {
            //pin became active
            //add it to the linked list 
            Transform prevPinTrans; LinkedListNode<Transform> node = activePins.First;
            while (node.Value != movingPin.transform) node = node.Next;
            prevPinTrans = node.CyclicPrevious().Value;
            Pin prevPin = pinTransformDict[prevPinTrans];
            //print pin order
            // LogActivePinNames();
            // print($"Will insert the new pin between {prevPin.transform.name} and {prevPin.nextPin.transform.name}");
            AddConnectedPinAfter(prevPin, movingPin);
            //debug
            // print($"Tried inserting {movingPin.transform.name} between {prevPin.transform.name} and {movingPin.nextPin.transform.name}");
        }
        else
        {
            // //pin became inactive
            // //remove it from the linked list
            // connectedActivePins.Remove(movingPin);
            // //remove the two segments that were connected to it
            // Transform segment1 = bandSegments[CollectionUtilities.IncrementWrap(
            //     Array.IndexOf(activePins, movingPin),-1,bandSegments.Count)];
            // Transform segment2 = bandSegments[CollectionUtilities.IncrementWrap(
            //     Array.IndexOf(activePins, movingPin),1,bandSegments.Count)];
            // bandSegments.Remove(segment1);
            // bandSegments.Remove(segment2);
            // ObjectPoolManager.Instance.InsertToPool(bandSegmentsPool, segment1.gameObject);
            // ObjectPoolManager.Instance.InsertToPool(bandSegmentsPool, segment2.gameObject);
        }
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
              "connectedActivePins: " + string.Join(", ", conActivePinsNames));;
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
    
    public enum MovingPinStatus
    {
        NotMoving,
        Moving
    }

    private void CreateBandSegment(LinkedListNode<Transform> node)
    {
        //adds a segment between this node and the cyclically-next node in the linked list
        Transform anchor1 = node.Value;
        Transform anchor2 = node.CyclicNext().Value;
        Transform segment = ObjectPoolManager.Instance.GetFromPool(bandSegmentsPool).transform;
        bandSegments.Add(segment);
        FollowTwoTransforms followScript = segment.GetComponent<FollowTwoTransforms>();
        followScript.target1 = anchor1;
        followScript.target2 = anchor2;
    }

    private class PinComparer : IComparer<Transform>
    {
        private Func<Transform, float> angle;

        public PinComparer(Vector2 centerOfActivePins)
        {
            angle = p => Mathf.Atan2(p.position.x - centerOfActivePins.x, p.position.y - centerOfActivePins.y);
        }

        public int Compare(Transform x, Transform y)
        {
            float angleX = angle(x); 
            float angleY = angle(y);
            if (angleX != angleY)
            {
                return angleX.CompareTo(angleY); //if the angles are different, sort by angle
            }
            else if (x.position.x != y.position.x) //if the angles are the same, sort by x position
            {
                return x.position.x.CompareTo(y.position.x);
            }
            else //if the x positions are the same, sort by y position
            {
                return x.position.y.CompareTo(y.position.y);
            }
        }
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
    public Pin nextPin; //the pin that follows this segment
    public Pin prevPin; //the pin that precedes this segment

    public Segment(string objectPoolName, Pin prevPin, Pin nextPin)
    {
        transform = ObjectPoolManager.Instance.GetFromPool(objectPoolName).transform;
        this.nextPin = nextPin;
        this.prevPin = prevPin;
        this.prevPin.nextSegment = this;
        this.nextPin.prevSegment = this;
        
        FollowTwoTransforms followScript = transform.GetComponent<FollowTwoTransforms>();
        followScript.target1 = prevPin.transform;
        followScript.target2 = nextPin.transform;
    }
}


