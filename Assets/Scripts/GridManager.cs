using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GridManager : ObjectPoolInterface
{
    //grid points
    private Queue<Transform> points = new();
    private int rowNum = 0;
    
    //grid settings
    [SerializeField] private int widthPts;
    private float rowDist; //the vertical distance between two rows
    [SerializeField] [Tooltip("The distance in meters between adjacent grid points")] private float gridLength;

    private void Start()
    {
        rowDist = gridLength * (float)Math.Sqrt(3f) / 2; //for triangular grid
        SpawnRow();
        SpawnRow();
        SpawnRow();
        SpawnRow();
        SpawnRow();
        SpawnRow();
        SpawnRow();
        SpawnRow();
    }

    private void SpawnRow()
    {
        if (rowNum % 2 == 0)
        {
            SpawnRow(widthPts);
        }
        else
        {
            SpawnRow(widthPts - 1);
        }
    }

    private void SpawnRow(int numPoints)
    {
        Vector3[] positions = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            positions[i].x = (-(numPoints - 1) * gridLength / 2) + i * gridLength;
            SpawnPoint(positions[i]);
        }
        
        rowNum++;
    }

    private void SpawnPoint(Vector3 position)
    {
        GameObject newPoint = objectPoolManager.GetFromPool(poolName);
        Transform newTransform = newPoint.transform;
        points.Enqueue(newTransform);
        newTransform.parent = transform;
        newTransform.localPosition = position + rowNum * rowDist * Vector3.up;
    }

    public Transform GetClosestPoint(Vector2 position)
    {
        Transform[] pointsArr = points.ToArray();
        float minValue = float.MaxValue;
        int minIndex = -1;
        for (int i = 0; i < points.Count; i++)
        {
            float dist = Vector3.Distance(pointsArr[i].position, position);
            if (dist < minValue)
            {
                minValue = dist;
                minIndex = i;
            }
        }

        if (minIndex == -1)
        {
            throw new Exception("Couldn't find closest point");
        }

        return pointsArr[minIndex];
    }
}
