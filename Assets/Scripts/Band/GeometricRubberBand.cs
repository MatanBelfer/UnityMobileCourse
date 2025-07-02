using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.ProBuilder.MeshOperations;

public class GeometricRubberBand : BaseManager
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
    private HashSet<BandSegment> segments = new(); //the segments that compose the band

    protected override void OnInitialize()
    {
        //initialize Pin dictionary
        foreach (Transform pinTrans in pins) pinTransformDict.Add(pinTrans, new BandVertex(pinTrans));
        
        InitializeBendsAndSegments();
        UpdateSegmentConnections();
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

    private void InitializeBendsAndSegments()
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
            RubberBandAnchor anchor = initialBand[i];
            Bend newBend = new Bend(anchor);
            bends.AddLast(newBend);
        }
    }
    
    protected override void OnReset()
    {
        Reset();
    }

    protected override void OnCleanup()
    {
        throw new NotImplementedException();
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        int index = 0;
        Dictionary<RubberBandAnchor, int> numBendsInAnchor = new();
        foreach (Bend bend in bends)
        {
            if (numBendsInAnchor.ContainsKey(bend.anchor)) numBendsInAnchor[bend.anchor]++;
            else numBendsInAnchor.Add(bend.anchor, 1);
            
            float offset = (numBendsInAnchor[bend.anchor] - 1) * 0.25f;
            string cw = bend.isClockwise ? "CW" : "CCW";
            Handles.Label(bend.anchor.transform.position + offset * Vector3.down,
                $"{bend.anchor.name}\nIndex: {index++}\n{cw}");
        }
    }
    #endif

    private void Update()
    {
        // UpdateBandSegments();
        CalculateIntersections();
        RemoveWrongBends();
        UpdateSegmentConnections();
        UpdateSegmentPositions();
    }

    private BandSegment GetSegmentFromPool(Bend prevBend)
    {
        BandSegment segment = GetSegmentFromPool();
        segment.prevBend = prevBend;
        return segment;
    }

    private BandSegment GetSegmentFromPool()
    {
        return ManagersLoader.Pool.GetFromPool(bandSegmentsPool).GetComponent<BandSegment>();
    }

    private void CalculateIntersections()
    {
        //change the structure of the band by adding in new bends when anchors intersect segments
        foreach (BandSegment segment in segments)
        {
            //check for intersections with all anchors other than the ones the segment is connected to
            Dictionary<RubberBandAnchor, (bool, float)> intersectingAnchors = 
                new(); //running list of the intersecting anchors
                       //along with their isCW and their dot product with the segment
            RubberBandAnchor start = segment.prevBend.anchor;
            RubberBandAnchor end = segment.nextBend.anchor;
            RubberBandAnchor[] relevantAnchors = allAnchors.Where(a =>
                (a != start) && (a != end)).ToArray();
            
            // print($"Segment connected to {start.name} and {end.name}\n" +
            //       $"will check intesections with anchors: " +
            //       $"{string.Join(", ", relevantAnchors.Select(a => a.name).ToArray())}");

            Vector2[] startPos = { start.previousPosition, start.currentPosition };
            Vector2[] endPos = { end.previousPosition, end.currentPosition };
            Vector2[] start2End = {endPos[0] - startPos[0], endPos[1] - startPos[1] };
            float[] sqrSegmentLength = {start2End[0].sqrMagnitude, start2End[1].sqrMagnitude};
            foreach (RubberBandAnchor anchor in relevantAnchors)
            {
                Vector2[] anchorPos = { anchor.previousPosition, anchor.currentPosition };
                //intersection happens when the anchor moves from one side of the segment to the other
                //while being "in front of it" for at least one of the two frames
                //it means the anchor can be projected onto the segment
                
                //first, check the projections
                float[] dotProd = {Vector2.Dot(start2End[0],(anchorPos[0] - startPos[0])), Vector2.Dot(start2End[1],(anchorPos[1] - startPos[1]))};
                bool skipAnchor = false;
                for (int i = 0; i < 2; i++)
                {
                    const float margin = 0f;
                    if (!(dotProd[i] > -margin && dotProd[i] < sqrSegmentLength[i] + margin))
                    {
                        skipAnchor = true;
                        break;
                    }
                }
                if (skipAnchor) continue;

                // print($"{anchor.name} has dot prods: " +
                //       $"{dotProd[0]}/{sqrSegmentLength[0]}, {dotProd[1]}/{sqrSegmentLength[1]}");
                
                //now check which side of the segment the anchor is in the two frames
                //using cross product
                Vector2[] start2Anchor = { anchorPos[0] - startPos[0], anchorPos[1] - startPos[1] };
                float[] crossProd =
                    { CrossProduct2d(start2End[0], start2Anchor[0]), CrossProduct2d(start2End[1], start2Anchor[1]) };
                bool[] leftSide = {crossProd[0] > 0, crossProd[1] > 0};
                if (leftSide[0] == leftSide[1]) continue; //the anchor stayed on the same side
                
                //print($"{anchor.name} on left side: {leftSide[0]}, {leftSide[1]}");
                
                intersectingAnchors.Add(anchor, (leftSide[1], dotProd[1]));
            }
            
            //if more than one anchor has intersected, handle them in order of dot product
            if (intersectingAnchors.Count > 1)
            {
                intersectingAnchors = intersectingAnchors.OrderBy(x => x.Value.Item2)
                    .ToDictionary(x => x.Key, x => x.Value);
            }
            
            //now add the new bends
            LinkedListNode<Bend> prevBend = bends.Find(segment.prevBend);
            foreach (KeyValuePair<RubberBandAnchor, (bool, float)> anchor in intersectingAnchors)
            {
                Bend newBend = new Bend(anchor.Key, anchor.Value.Item1);
                bends.AddAfter(prevBend, newBend);
                prevBend = prevBend.Next;
            }
        }
    }

    private void RemoveWrongBends()
    {
        //removes each bend that shouldn't affect the Band
        LinkedListNode<Bend> node = bends.First;
        while (node != null)
        {
            Bend bend = node.Value;
            Vector2 prevStart = node.PreviousOrLast().Value.anchor.currentPosition;
            Vector2 prevStart2NextEnd = node.NextOrFirst().Value.anchor.currentPosition - prevStart;
            Vector2 prevStart2Here = bend.anchor.currentPosition - prevStart;
            float crossProd = CrossProduct2d(prevStart2NextEnd, prevStart2Here);

            float stickage = 0f;
            if (bend.isClockwise && crossProd < -stickage || !bend.isClockwise && crossProd > stickage)
            {
                LinkedListNode<Bend> nextNode = node.Next;
                print($"removing {node.Value.anchor.name} because it has\n" +
                       $"isCW = {bend.isClockwise}, crossProd = {crossProd}");
                bends.Remove(node);
                node = nextNode;
                continue;
            }
            node = node.Next;
        }
    }
    
    private float CrossProduct2d(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }
    
    private void UpdateSegmentConnections()
    {
        //add new segments and update connections
        for (LinkedListNode<Bend> node = bends.First; node != null; node = node.Next)
        {
            Bend bend = node.Value;
            Bend nextBend = node.NextOrFirst().Value;
            BandSegment nextSegment = bend.nextSegment;
            if (!nextSegment)
            {
                nextSegment = GetSegmentFromPool();
                segments.Add(nextSegment);
            }

            bend.nextSegment = nextSegment;
            nextSegment.prevBend = bend;
            nextSegment.nextBend = nextBend;
            nextBend.prevSegment = nextSegment;
        }
        
        //remove segments that are no longer needed
        segments.RemoveWhere(seg =>
        {
            bool remove = !bends.Select(bend => bend.nextSegment).Contains(seg);
            if (remove) ManagersLoader.Pool.InsertToPool(bandSegmentsPool, seg.gameObject);
            return remove;
        });
    }

    private void UpdateSegmentPositions()
    {
        foreach (BandSegment segment in segments)
        {
            Bend[] bends = {segment.prevBend, segment.nextBend};
            bool[] isCW = bends.Select(bend => bend.isClockwise).ToArray();
            Vector2[] anchorPos = bends.Select(bend => bend.anchor.currentPosition).ToArray();
            Vector2 direction = (anchorPos[1] -  anchorPos[0]).normalized;
            Vector2 left = new Vector2(-direction.y, direction.x); //rotated left 90 degrees
            float[] baseRadius = bends.Select(bend => bend.anchor.baseRadius).ToArray();

            segment.position1 = anchorPos[0] + (isCW[0] ? 1f : -1f) * baseRadius[0] * left;
            segment.position2 = anchorPos[1] + (isCW[1] ? 1f : -1f) * baseRadius[1] * left;
        }
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