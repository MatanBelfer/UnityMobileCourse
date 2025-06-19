using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridManager : ObjectPoolInterface
{
    //grid settings
    [SerializeField] private GridParameters gridParameters;

    [Tooltip("The distance in meters between adjacent grid points")]
    public float gridLength;

    public static GridManager Instance { get; private set; }

    private HashSet<Transform> activePoints = new HashSet<Transform>();

    //privates
    private bool _showDebugInfo = true; // Set to false to disable debug visualization
    private Color _gizmoTextColor = Color.yellow; // You can change the color to make it more visible

    //grid structure
    private Queue<Transform[]> _gridRows = new();
    private int _rowNum;//1 based (not zero-based)
    private float _rowDist;

    protected override void Awake()
    {
        base.Awake();  // Call the base class Awake first
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeWhenReady());
    }

    private System.Collections.IEnumerator InitializeWhenReady()
    {
        //TODO: yield return new WaitUntil(() => ManagersLoader.IsInitialized); instead of while
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


        _rowDist = gridLength * (float)Math.Sqrt(3f) / 2;

        InitializeGrid();
    }

    private void InitializeGrid()
    {
        _rowNum = 0;
        for (int i = 0; i < gridParameters.initialRowCount; i++)
        {
            SpawnRow();
        }
    }

    public void Update()
    {
        transform.Translate(Vector3.down * (gridParameters.gridSpeed * Time.deltaTime));
    }

    public void SpawnNewRow()
    {
        SpawnRow();
    }

    public int GetCurrentRowPointCount()
    {
        int nextRow = _rowNum;
        return (nextRow % 2 == 0) ? gridParameters.pointsPerRow : gridParameters.pointsPerRow - 1;
    }

    private void SpawnRow()
    {
        if (_rowNum % 2 == 0)
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
            positions[i].x = (-(numPoints - 1) * gridParameters.pointSpacing / 2) + i * gridParameters.pointSpacing;
            rowPoints[i] = SpawnPoint(positions[i]);

            // Only spawn spikes if we're at or past the trapStartRow
            if (_rowNum >= gridParameters.trapStartRow && Random.value < gridParameters.spikeSpawnChance)
            {
                
                SpawnSpike(positions[i]);
            }
        }

        _gridRows.Enqueue(rowPoints);
        _rowNum++;
    }


    private void SpawnSpike(Vector3 position)
    {
        
        //turned off for testing
        // print("SpawnSpike has been turned off");
        // return;
        GameObject spike = objectPoolManager.GetFromPool(gridParameters.spikePoolName);
        spike.transform.parent = transform;
        spike.transform.localPosition = position + _rowNum * _rowDist * Vector3.up;
        spike.SetActive(true);
    }

    public void RemovePoint(Transform point)
    {
        var currentRows = _gridRows.ToArray();
        for (int i = 0; i < currentRows.Length; i++)
        {
            var row = currentRows[i];
            for (int j = 0; j < row.Length; j++)
            {
                if (row[j] == point)
                {
                    row[j] = null;
                    activePoints.Remove(point);
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
        newTransform.localPosition = position + _rowNum * _rowDist * Vector3.up;
        newPoint.SetActive(true);
        activePoints.Add(newTransform);
        return newTransform;
    }

    public Transform GetClosestPoint(Vector2 position, out int chosenRow)
    {
        Transform closestPoint = null;
        float minDistance = float.MaxValue;
        chosenRow = -1;

        int currentRowInQueue = 0;
        foreach (var row in _gridRows)
        {
            foreach (var point in row)
            {
                if (point == null) continue;

                float dist = Vector3.Distance(point.position, position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestPoint = point;
                    chosenRow = _rowNum - _gridRows.Count + currentRowInQueue + 1;
                }
            }

            currentRowInQueue++;
        }

        if (closestPoint == null)
        {
            throw new Exception("Couldn't find closest point");
        }
        
        return closestPoint;
    }

    public Transform GetClosestPoint(Vector2 position)
    {
        return GetClosestPoint(position, out var _);
    }


    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_showDebugInfo || !Application.isPlaying) return;

        var rows = _gridRows.ToArray();
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i].Length > 0 && rows[i][0] != null)
            {
                // Calculate the position for the label (slightly to the left of the first point in the row)
                Vector3 labelPosition = rows[i][0].position + Vector3.left * 0.5f;

                // Draw the row number
                Handles.color = _gizmoTextColor;
                Handles.Label(labelPosition, $"Row: {i}");

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
    #endif


    /*
     * public methods to interact with the grid
     */

    public Transform GetPointAt(int rowIndex, int columnIndex)
    {
        if (!IsValidPosition(rowIndex, columnIndex))
            return null;

        var rows = _gridRows.ToArray();
        return rows[rowIndex][columnIndex];
    }

    public bool IsValidPosition(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0 || rowIndex >= _gridRows.Count)
            return false;

        var rows = _gridRows.ToArray();
        return columnIndex >= 0 && columnIndex < rows[rowIndex].Length;
    }


    public void ClearPoint(GameObject point)
    {
        if (point != null)
        {
            RemovePoint(point.transform);

            objectPoolManager.InsertToPool(poolName, point);
        }
    }

    public void ClearAllPoints()
    {
        foreach (var point in activePoints)
        {
            if (point != null)
            {
                objectPoolManager.InsertToPool(poolName, point.gameObject);
            }
        }

        activePoints.Clear();
        _gridRows.Clear();
        _rowNum = 0;
    }

    public void ClearSpike(GameObject spike)
    {
        if (spike != null && objectPoolManager != null)
        {
            // Remove from grid first
            spike.transform.SetParent(null);
            spike.SetActive(false);
            objectPoolManager.InsertToPool(gridParameters.spikePoolName, spike);
        }
    }
    
    public void ReturnObjectToPool(GameObject obj)
    {
        
        
        objectPoolManager.InsertToPool(obj.GetComponent<ObjectPoolInterface>()?.poolName, obj);
    }
}