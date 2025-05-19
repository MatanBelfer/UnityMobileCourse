using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : ObjectPoolInterface
{
    
    private bool showDebugInfo = true;  // Set to false to disable debug visualization
    private Color gizmoTextColor = Color.yellow;  // You can change the color to make it more visible

    //privates
    private int currentRow = 0;

    //grid structure
    private Queue<Transform[]> gridRows = new();
    private int rowNum = 0;

    //grid settings
    [SerializeField] private GridParameters gridParameters;
    private int numPointsInRow = 5;
    private float pointSpacing = 1f;
    private float rowDist;
    private float scrollSpeed = 0.2f;

    [Tooltip("The distance in meters between adjacent grid points")]
    public float gridLength;

    private void Start()
    {
        StartCoroutine(InitializeWhenReady());
    }

    private System.Collections.IEnumerator InitializeWhenReady()
    {
        while (!ManagersLoader.IsInitialized)
        {
            yield return null;
        }

        objectPoolManager = ObjectPoolManager.Instance;
        if (objectPoolManager == null)
        {
            Debug.LogError("ObjectPoolManager not found! Make sure it's set up in the scene.");
            yield break;
        }

        // Initialize grid parameters
        numPointsInRow = gridParameters != null ? gridParameters.pointsPerRow : 5;
        pointSpacing = gridParameters != null ? gridParameters.pointSpacing : 0.5f;
        rowDist = gridLength * (float)System.Math.Sqrt(3f) / 2;
        scrollSpeed = gridParameters != null ? gridParameters.gridSpeed : 0.2f;

        InitializeGrid();
    }

    private void InitializeGrid()
    {
        rowNum = 0;
        for (int i = 0; i < gridParameters.initialRowCount; i++)
        {
            SpawnRow();
        }
    }

    public void Update()
    {
        transform.Translate(Vector3.down * gridParameters.gridSpeed * Time.deltaTime);
    }

    public void SpawnNewRow()
    {
        SpawnRow();
    }

    public int GetCurrentRowPointCount()
    {
        int nextRow = rowNum;
        return (nextRow % 2 == 0) ? gridParameters.pointsPerRow : gridParameters.pointsPerRow - 1;
    }

    private void SpawnRow()
    {
        if (rowNum % 2 == 0)
        {
            SpawnRow(gridParameters.pointsPerRow);
        }
        else
        {
            SpawnRow(gridParameters.pointsPerRow - 1);
        }
    }

    private void SpawnRow(int numPoints)
    {
        Transform[] rowPoints = new Transform[numPoints];
        Vector3[] positions = new Vector3[numPoints];

        for (int i = 0; i < numPoints; i++)
        {
            positions[i].x = (-(numPoints - 1) * pointSpacing / 2) + i * pointSpacing;
            rowPoints[i] = SpawnPoint(positions[i]);

            // Only spawn spikes if we're at or past the trapStartRow
            if (rowNum >= gridParameters.trapStartRow && Random.value < gridParameters.spikeSpawnChance)
            {
                SpawnSpike(positions[i]);
            }
        }

        gridRows.Enqueue(rowPoints);
        rowNum++;
    }


    private void SpawnSpike(Vector3 position)
    {
        GameObject spike = objectPoolManager.GetFromPool(gridParameters.spikePoolName);
        spike.transform.parent = transform;
        spike.transform.localPosition = position + rowNum * rowDist * Vector3.up;
        spike.SetActive(true);
    }

    public void RemovePoint(Transform point)
    {
        var currentRows = gridRows.ToArray();
        for (int i = 0; i < currentRows.Length; i++)
        {
            var row = currentRows[i];
            for (int j = 0; j < row.Length; j++)
            {
                if (row[j] == point)
                {
                    row[j] = null;
                    return;
                }
            }
        }
    }

    private Transform SpawnPoint(Vector3 position)
    {
        GameObject newPoint = objectPoolManager.GetFromPool(poolName);
        Transform newTransform = newPoint.transform;
        newTransform.parent = transform;
        newTransform.localPosition = position + rowNum * rowDist * Vector3.up;
        newPoint.SetActive(true);
        return newTransform;
    }

    public Transform GetClosestPoint(Vector2 position)
    {
        Transform closestPoint = null;
        float minDistance = float.MaxValue;

        foreach (var row in gridRows)
        {
            foreach (var point in row)
            {
                if (point == null) continue;

                float dist = Vector3.Distance(point.position, position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestPoint = point;
                }
            }
        }

        if (closestPoint == null)
        {
            throw new Exception("Couldn't find closest point");
        }

        return closestPoint;
    }


    private void OnDrawGizmos()
    {
        if (!showDebugInfo || !Application.isPlaying) return;

        var rows = gridRows.ToArray();
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i].Length > 0 && rows[i][0] != null)
            {
                // Calculate position for the label (slightly to the left of the first point in the row)
                Vector3 labelPosition = rows[i][0].position + Vector3.left * 0.5f;
                
                // Draw the row number
                UnityEditor.Handles.color = gizmoTextColor;
                UnityEditor.Handles.Label(labelPosition, $"Row: {i}");

                // Optionally, draw a line connecting all points in the row
                Gizmos.color = Color.cyan;
                for (int j = 0; j < rows[i].Length - 1; j++)
                {
                    if (rows[i][j] != null && rows[i][j + 1] != null)
                    {
                        Gizmos.DrawLine(rows[i][j].position, rows[i][j + 1].position);
                    }
                }
            }
        }
    }

    
    
    
    /*
     * public methods to interact with the grid
     */

    public Transform GetPointAt(int rowIndex, int columnIndex)
    {
        if (!IsValidPosition(rowIndex, columnIndex))
            return null;

        var rows = gridRows.ToArray();
        return rows[rowIndex][columnIndex];
    }

    public bool IsValidPosition(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0 || rowIndex >= gridRows.Count)
            return false;

        var rows = gridRows.ToArray();
        return columnIndex >= 0 && columnIndex < rows[rowIndex].Length;
    }
}