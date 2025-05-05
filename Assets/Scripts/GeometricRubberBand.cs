using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class GeometricRubberBand : ObjectPoolInterface
{
    //calculates the lines composing the rubber band, using anchors and obstacles
    //the calculated area will exclude the obstacles and will be as small as possible, composed of only straight lines 
    //between the anchors and obstacles
    
    [SerializeField] private Transform[] pins; //the four pins the player moves
    private List<(Transform,Transform)> connections = new(); //the connections between all transforms that affect the band
    private List<Transform> activePins = new(); //the pins the band is touching
    private List<List<Transform>> anchors = new(); //where the band connects - two per pin and more for the obstacles. these are stored in an object pool
    
    private Vector2 centerOfActivePins; //the center of the active pins
    [SerializeField] private float pinRadius; //the radius of the pins

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
        
        //find pins that define the shape of the band
        activePins = pins.Where(p => PointWithinBounds(p) != -1).ToList();
        
        //find center of active pins
        centerOfActivePins = activePins.Select(p => p.position).
            Aggregate((a, b) => a + b) / pins.Length;
        
        //sort active pins counterclockwise
        activePins = activePins.OrderBy(p => Mathf.Atan2(p.position.x - centerOfActivePins.x, p.position.y - centerOfActivePins.y)).ToList();
        
        //spawn anchors
        foreach (Transform pin in activePins)
        {
            List<Transform> newAnchors = new();
            for (int i = 0; i < 2; i++)
            {
                Transform anchor = objectPoolManager.GetFromPool(poolName).transform;
                anchor.parent = pin;
                newAnchors.Add(anchor);
            }
            anchors.Add(newAnchors);
        }
        
        //move anchors to edges of pins
        for (int i = 0; i < activePins.Count; i++)
        {
            //find outside direction
            Vector2 pinPos = activePins[i].position;
            Vector2 nextPinPos = activePins[(i + 1) % activePins.Count].position;
            Vector2 outSideDir =
                (ClosestPointOnLine(centerOfActivePins, pinPos, nextPinPos, out _) - centerOfActivePins).normalized;
            
            //move the anchors
            Vector2 moveAmount = outSideDir * 0.5f * pinRadius;
            anchors[i][0].Translate(moveAmount);
            anchors[(i + 1) % activePins.Count][1].Translate(moveAmount);
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

    private void Update()
    {
        
    }

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

    // private struct Line
    // {
    //     Transform start;
    //     Transform end;
    //     public Line(Transform start, Transform end)
    //     {
    //         this.start = start;
    //         this.end = end;
    //     }
    // }
}
