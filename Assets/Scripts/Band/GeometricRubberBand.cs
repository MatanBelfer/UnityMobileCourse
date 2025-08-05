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

    private LinkedList<Transform> activePins; //the pins the band is touching
    private Pin movingPin; //the currently moving pin

    private HashSet<Pin>
        connectedActivePins =
            new(); //like a linkedlist: each Pin has references to the next and previous Pin, and the next and previous Segments
    //whenever you update this list, you must also spawn or despawn segments accordingly
    //it always remains circular: the last pin connects to the first
    
    private Dictionary<Transform, Pin> pinTransformDict = new(); //a dictionary that maps transforms to Pins

    private List<Transform> bandSegments = new(); //the segments of the band - used for animation
    [SerializeField] private string bandSegmentsPool;

    private Vector2 centerOfActivePins; //the center of the active pins
    // [SerializeField] private float pinRadius; //the radius of the pins
    
    public static Color bandColor { get; private set; } = Color.black;//from skin

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

    public void ApplySkinGraphics()
    {
        SkinAsset skin = ManagersLoader.Shop.equippedSkinAsset;
        Color pinColor;
        if (skin == null)
        {
            pinColor = Color.white;
            bandColor = Color.black;
        }
        else
        {
            pinColor = skin.pinColor;
            bandColor = skin.bandColor;
        }
        
        foreach (Transform pin in pins)
        {
            pin.GetComponentInChildren<MeshRenderer>().material.color = pinColor;
        }
    }

    private void Start()
    {
        ApplySkinGraphics();
    }

    private void Update()
    {
        UpdateBandSegments();
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
        // ObjectPoolManager.Instance.ReturnToPool(bandSegmentsPool, oldSegment.transform.gameObject);

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
            ManagersLoader.Pool.ReturnToPool(bandSegmentsPool, segmentToRemove.gameObject);
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
                ManagersLoader.Pool.ReturnToPool(bandSegmentsPool, segment.gameObject);
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


    public enum MovingPinStatus
    {
        NotMoving,
        Moving
    }

    private Vector2 OuterPoint()
    {
        return Vector2.right * pins.Max(t => t.position.x) + Vector2.up * pins.Max(t => t.position.y);
    }
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
        //give it the correct color
        transform.GetComponent<MeshRenderer>().material.color = GeometricRubberBand.bandColor;
//        Debug.Log($"Constructed new segment with color {GeometricRubberBand.bandColor}");

        followScript = transform.GetComponent<FollowTwoTransforms>();
        followScript.target1 = prevPin.transform;
        followScript.target2 = nextPin.transform;

        this.nextPin = nextPin;
        this.prevPin = prevPin;
        this.prevPin.nextSegment = this;
        this.nextPin.prevSegment = this;
    }
}