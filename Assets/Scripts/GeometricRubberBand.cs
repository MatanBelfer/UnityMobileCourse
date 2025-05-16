using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.Serialization;
using UnityEngine.Splines;

public class GeometricRubberBand : ObjectPoolInterface
{
    //calculates the lines composing the rubber band, using anchors and obstacles
    //the calculated area will exclude the obstacles and will be as small as possible, composed of only straight lines 
    //between the anchors and obstacles
    
    [SerializeField] private Transform[] pins; //the four pins the player moves
    private List<(Transform,Transform)> connections = new(); //the connections between all transforms that affect the band
    private List<Transform> activePins = new(); //the pins the band is touching
    private List<Transform> anchors = new(); //where the band connects - two per pin and more for the obstacles. these are stored in an object pool
    // private List<Transform> midPoints = new(); //the midpoints of the band segments - used for animation
    private List<Transform> bandSegments = new(); //the segments of the band - used for animation
    [SerializeField] private string bandSegmentsPool;
    
    private Vector2 centerOfActivePins; //the center of the active pins
    // [SerializeField] private float pinRadius; //the radius of the pins
    
    // [SerializeField] private SplineContainer splineContainer;
    // [SerializeField] private float splineTangentLengthRatio;

    private void Start()
    {
        //initialize connections
        for (int i = 0; i < pins.Length - 1; i++)
        {
            for (int j = i + 1; j < pins.Length; j++)
            {
                connections.Add((pins[i], pins[j]));
            }
        }
        
        CalculateActivePins();
        
        //spawn anchors
        foreach (Transform pin in activePins)
        {
            Transform anchor = objectPoolManager.GetFromPool(poolName).transform;
            anchor.parent = pin;
            anchor.localPosition = Vector3.zero;
            anchors.Add(anchor);
        }

        //move anchors to edges of pins
        // for (int i = 0; i < activePins.Count; i++)
        // {
        //     //find outside direction
        //     Vector2 pinPos = activePins[i].position;
        //     Vector2 nextPinPos = activePins[(i + 1) % activePins.Count].position;
        //     Vector2 outSideDir =
        //         (ClosestPointOnLine(centerOfActivePins, pinPos, nextPinPos, out _) - centerOfActivePins).normalized;
        //     
        //     //move the anchors
        //     Vector2 moveAmount = outSideDir * pinRadius;
        //     anchors[i][0].Translate(moveAmount);
        //     anchors[(i + 1) % activePins.Count][1].Translate(moveAmount);
        // }
        
        // //setup midpoints for animation
        // for (int i = 0; i < anchors.Count; i++)
        // {
        //     midPoints.Add(objectPoolManager.GetFromPool(poolName).transform);
        //     midPoints[i].position = (anchors[i].position + anchors[(i + 1) % anchors.Count].position) / 2;
        // }
        ;
        //spawn band segments
        for (int i = 0; i < anchors.Count; i++)
        {
	        Transform segment = objectPoolManager.GetFromPool(bandSegmentsPool).transform;
	        bandSegments.Add(segment);
	        FollowTwoTransforms followScript = segment.GetComponent<FollowTwoTransforms>();
	        followScript.target1 = anchors[i];
	        followScript.target2 = anchors[(i + 1) % anchors.Count];
	        followScript.follow = true;
        }
        
        // for (int i = 0; i < activePins.Count; i++)
        // {
        //     // Vector3 startPos = anchors[i][1].position; 
        //     // Vector3 endPos = anchors[(i + 1) % activePins.Count][0].position;
        //     // Vector3 midPoint = (startPos + endPos) / 2;
        //     // Vector3 tangent = (endPos - startPos) / 2 * splineTangentLengthRatio;
        //     //
        //     // BezierKnot[] knots = new BezierKnot[3];
        //     // knots[0] = new BezierKnot(startPos, float3.zero, (float3)tangent);
        //     // knots[1] = new BezierKnot(midPoint, -tangent, tangent);
        //     // knots[2] = new BezierKnot(endPos, -tangent, float3.zero);
        //     //
        //     // splineContainer[i].Knots = knots;
        // }
        
        
    }

    private void CalculateActivePins()
    {
        //find pins that define the shape of the band
        activePins = pins.Where(p => PointWithinBounds(p) == -1).ToList();
        
        //find center of active pins
        centerOfActivePins = activePins.Select(p => p.position).
            Aggregate((a, b) => a + b) / pins.Length;
        
        //sort active pins counterclockwise
        activePins = activePins.OrderBy(_ => _, new PinComparer(centerOfActivePins)).ToList();
    }

    private List<Transform> PinsOnConvexHull(List<Transform> points)
    {
        if (points.Count <= 3)
            return points;

        List<Transform> hull = new List<Transform>();
    
        // Find the leftmost point (with lowest x-coordinate)
        int leftmostIndex = 0;
        for (int i = 1; i < points.Count; i++)
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
            hull.Add(points[currentPoint]);
        
            // Find next point with largest counterclockwise angle
            nextPoint = (currentPoint + 1) % points.Count;
        
            for (int i = 0; i < points.Count; i++)
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

    private void Update()
    {
        
        //draw lines between active pins
        for (int i = 0; i < activePins.Count; i++)
        {
            Transform anchor1 = activePins[i];
            Transform anchor2 = activePins[(i + 1) % activePins.Count];
            Vector2 anchor1Pos = anchor1.position;
            Vector2 anchor2Pos = anchor2.position;
            Debug.DrawLine(anchor1Pos, anchor2Pos, Color.red, 0.05f);
        }
        
        //update active pins (not efficiently)
        activePins = PinsOnConvexHull(pins.ToList());;
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
	
    private int PointWithinBounds(Transform pointTransform)
    {
        //if the point is inside the bounds defined by connections, return the index of the closest bound, otherwise return -1
        Vector2 point = pointTransform.position;
        Vector2 pointOutsideBounds = OuterPoint();
        
        //count intersections of point<->pointOutsideBounds and the connections 
        int totalIntersections = 0;
        int indexClosestLine = -1;
        float minDist = float.MaxValue;
        for (int i = 0; i < connections.Count; i++)
        {
            //ignore the lines drawn from this point
            if (connections[i].Item1 == pointTransform || connections[i].Item2 == pointTransform)
            {
                continue;
            }
            
            //update minimum
            float dist = Mathf.Abs(SignedDistPointLine(point, connections[i].Item1.position, connections[i].Item2.position));
            if (dist < minDist)
            {
                minDist = dist;
                indexClosestLine = i;
            }

            //count intersections
            if (DoLinesIntersect(point, pointOutsideBounds, connections[i].Item1.position,
                    connections[i].Item2.position))
            {
                totalIntersections++;
            }
        }

        return totalIntersections % 2 == 1 ? indexClosestLine : -1;
    }

    private Vector2 OuterPoint()
    {
        return Vector2.right * pins.Max(t => t.position.x) + Vector2.up * pins.Max(t => t.position.y);
    }

    private bool DoLinesIntersect(Vector2 lineStart1, Vector2 lineEnd1, Vector2 lineStart2, Vector2 lineEnd2)
    {
        return !PointsOnSameSide(lineStart1, lineEnd1, lineStart2, lineEnd2) && !PointsOnSameSide(lineStart2, lineEnd2, lineStart1, lineEnd1);
    }

    private bool PointsOnSameSide(Vector2 point1, Vector2 point2, Vector2 lineStart, Vector2 lineEnd)
    {
        return SignedDistPointLine(point1, lineStart, lineEnd) * SignedDistPointLine(point2, lineStart, lineEnd) >= 0;
    }
    
    private float SignedDistPointLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        //returns a signed distance between a point and a line
        Vector2 closestPoint = ClosestPointOnLine(point, lineStart, lineEnd, out Vector2 lineDir);
        return Vector3.Cross(lineDir, point - closestPoint).z;
    }

    private Vector2 ClosestPointOnLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd, out Vector2 lineDir)
    {
        //point on the line defined by lineStart and lineEnd which is closest to point. also outputs the direction of the line in lineDir.
        Vector2 lineVec = lineEnd - lineStart;
        lineDir = lineVec.normalized;
        Vector2 closestPoint = lineStart + Vector2.Dot(point - lineStart, lineDir) * lineDir;
        return closestPoint;
    }
}
