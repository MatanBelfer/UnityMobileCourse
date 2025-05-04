using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GeometricRubberBand : MonoBehaviour
{
    //calculates the lines composing the rubber band, using anchors and obstacles
    //the calculated area will exclude the obstacles and will be as small as possible, composed of only straight lines 
    //between the anchors and obstacles
    
    [SerializeField] private Transform[] anchors;
    private List<(Transform,Transform)> connections = new();

    private void Start()
    {
        //initialize connections
        // print(NChooseK(anchors.Length, 2));
        // connections = new List<(Transform,Transform)>(NChooseK(anchors.Length, 2));
        
        for (int i = 0; i < anchors.Length - 1; i++)
        {
            for (int j = i + 1; j < anchors.Length; j++)
            {
                connections.Add((anchors[i], anchors[j]));
            }
        }
    }

    private int NChooseK(int n, int k)
    {
        return Factorial(n) / (Factorial(k) * Factorial(n - k));
    }

    private int Factorial(int n)
    {
        return n == 0 ? 1 : n * Factorial(n - 1);
    }

    private void Update()
    {
        string msg = "";
        for (int i = 0; i < anchors.Length; i++)
        {
            msg += $"{i}: {PointWithinBounds(anchors[i])}, ";
        }
        print(msg);
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
        return Vector2.right * anchors.Max(t => t.position.x) + Vector2.up * anchors.Max(t => t.position.y);
    }

    private bool DoLinesIntersect(Vector2 lineStart1, Vector2 lineEnd1, Vector2 lineStart2, Vector2 lineEnd2)
    {
        return !PointsOnSameSide(lineStart1, lineEnd1, lineStart2, lineEnd2) && !PointsOnSameSide(lineStart2, lineEnd2, lineStart1, lineEnd1);
    }

    private bool PointsOnSameSide(Vector2 point1, Vector2 point2, Vector2 lineStart, Vector2 lineEnd)
    {
        return SignedDistPointLine(point1, lineStart, lineEnd) * SignedDistPointLine(point2, lineStart, lineEnd) > 0;
    }
    
    private float SignedDistPointLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        //returns a signed distance between a point and a line
        Vector2 lineVec = lineEnd - lineStart;
        Vector2 lineDir = lineVec.normalized;
        Vector2 closestPoint = lineStart + Vector2.Dot(point - lineStart, lineDir) * lineDir;
        return Vector3.Cross(lineDir, point - closestPoint).z;
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
