using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridManager : BaseManager
{
    //grid settings
    [SerializeField] private GridParameters[] gridParametersList;
    private GridParameters gridParameters;

    [Tooltip("The distance in meters between adjacent grid points")]
    public float gridLength;

    private HashSet<Transform> activePoints = new HashSet<Transform>();

    //privates
    private bool _showDebugInfo = true; // Set to false to disable debug visualization
    private Color _gizmoTextColor = Color.yellow; // You can change the color to make it more visible

    //grid structure
    private Queue<Transform[]> _gridRows = new();
    private int _rowNum; //1 based (not zero-based)
    private float _rowDist;
    private bool _isGridInitialized = false;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnInitialize()
    {
        InitializeGridParameters();

        // Wait for ManagersLoader to be ready, then initialize synchronously
        if (ManagersLoader.IsInitialized && ManagersLoader.Pool != null)
        {
            InitializeGridNow();
        }
        else
        {
            // If not ready yet, start the coroutine
            StartCoroutine(InitializeWhenReady());
        }
    }

    protected override void OnReset()
    {
        // Reset grid state
        _isGridInitialized = false;
        _gridRows.Clear();
        activePoints.Clear();
        _rowNum = 0;

        // Reinitialize
        OnInitialize();
    }

    protected override void OnCleanup()
    {
        // Return all active objects to pools
        foreach (var point in activePoints.ToList())
        {
            if (point != null && point.gameObject != null)
            {
                ReturnObjectToPool(point.gameObject);
            }
        }

        activePoints.Clear();
        _gridRows.Clear();
        _isGridInitialized = false;
        _rowNum = 0;
    }

    private void InitializeGridParameters()
    {
        string difficultyKey = "difficulty";
        if (PlayerPrefs.HasKey(difficultyKey) && gridParametersList != null && gridParametersList.Length > 0)
        {
            int difficultyIndex = PlayerPrefs.GetInt(difficultyKey);
            if (difficultyIndex >= 0 && difficultyIndex < gridParametersList.Length)
            {
                gridParameters = gridParametersList[difficultyIndex];
            }
            else
            {
                gridParameters = gridParametersList[0]; // Default to first parameters
            }
        }
        else if (gridParametersList != null && gridParametersList.Length > 0)
        {
            gridParameters = gridParametersList[0]; // Default to first parameters
        }
    }

    private System.Collections.IEnumerator InitializeWhenReady()
    {
        // Wait for ManagersLoader to be initialized
        yield return new WaitUntil(() => ManagersLoader.IsInitialized);

        if (ManagersLoader.Pool == null)
        {
            Debug.LogError("ObjectPoolManager not found! Make sure it's set up in the scene.");
            yield break;
        }

        InitializeGridNow();
    }

    private void InitializeGridNow()
    {
        if (_isGridInitialized) return;

        if (gridParameters == null)
        {
            Debug.LogError("GridParameters not set! Make sure to assign grid parameters in the inspector.");
            return;
        }

        _rowDist = gridLength * (float)Math.Sqrt(3f) / 2;
        InitializeGrid();
        _isGridInitialized = true;
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
        // Only update if grid is properly initialized
        if (!_isGridInitialized || gridParameters == null) return;

        transform.Translate(Vector3.down * (gridParameters.gridSpeed * Time.deltaTime));
    }

    public void SpawnNewRow()
    {
        if (!_isGridInitialized || gridParameters == null) return;
        SpawnRow();
    }

    public int GetCurrentRowPointCount()
    {
        if (gridParameters == null) return 0;

        int nextRow = _rowNum;
        return (nextRow % 2 == 0) ? gridParameters.pointsPerRow : gridParameters.pointsPerRow - 1;
    }

    private void SpawnRow()
    {
        if (gridParameters == null) return;

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
        if (gridParameters == null || ManagersLoader.Pool == null) return;

        Transform[] rowPoints = new Transform[numPoints];
        Vector3[] positions = new Vector3[numPoints];
        GridPoint _Point;
        for (int i = 0; i < numPoints; i++)
        {
            positions[i].x = (-(numPoints - 1) * gridParameters.pointSpacing / 2) + i * gridParameters.pointSpacing;
            _Point =  SpawnPoint(positions[i]).GetComponent<GridPoint>();
            // rowPoints[i] = SpawnPoint(positions[i]);
            rowPoints[i] = _Point.transform;
            // rowPoints[i].GetComponent<GridPoint>().column = i;
            _Point.column = i;
            
            
            // Only spawn spikes if we're at or past the trapStartRow
            if (_rowNum >= gridParameters.trapStartRow && Random.value < gridParameters.spikeSpawnChance)
            {
                SpawnSpike(positions[i]);
                _Point.isBlocked = true;
                HidePoint(rowPoints[i]);
            }
        }

        _gridRows.Enqueue(rowPoints);
        _rowNum++;
    }

    private void SpawnSpike(Vector3 position)
    {
        if (gridParameters == null || ManagersLoader.Pool == null) return;
        GameObject spike = ManagersLoader.Pool.GetFromPool(gridParameters.spikePoolName);
        spike.transform.parent = transform;
        spike.transform.localPosition = position + _rowNum * _rowDist * Vector3.up;

        spike.SetActive(true);
    }


    private void HidePoint(Transform point)
    {
        point.gameObject.SetActive(false);
    }

    public void RemovePoint(Transform point)
    {
        Debug.Log($"removing point from grid {point.gameObject.name}");
        var currentRows = _gridRows.ToArray();
        for (int i = 0; i < currentRows.Length; i++)
        {
            var row = currentRows[i];
            for (int j = 0; j < row.Length; j++)
            {
                if (row[j] == point)
                {
                    Debug.Log("found point to remove");
                    activePoints.Remove(point);
                    ReturnObjectToPool(point.gameObject);
                    row[j] = null;
                    return;
                }
            }
        }
    }

    private Transform SpawnPoint(Vector3 position)
    {
        if (gridParameters == null || ManagersLoader.Pool == null) return null;

        var newPoint = ManagersLoader.Pool.GetFromPool(gridParameters.pointPoolName);
        var newTransform = newPoint.transform;

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
        if (!IsValidPosition(rowIndex, columnIndex) )
            return null;

        var rows = _gridRows.ToArray();
        return rows[rowIndex][columnIndex];
    }

    public bool IsPointBlocked(int rowIndex, int columnIndex)
    {
        return GetPointAt(rowIndex, columnIndex)?.GetComponent<GridPoint>().isBlocked ?? false;
    }
    public bool IsValidPosition(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0 || rowIndex >= _gridRows.Count)
            return false;
        
        var rows = _gridRows.ToArray();
        return columnIndex >= 0 && columnIndex < rows[rowIndex].Length;
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        if (obj == null || ManagersLoader.Pool == null) return;

        var poolInterface = obj.GetComponent<ObjectPoolInterface>();
        if (poolInterface != null && !string.IsNullOrEmpty(poolInterface.poolName))
        {
            ManagersLoader.Pool.InsertToPool(poolInterface.poolName, obj);
        }
    }
    
    /// <summary>
    /// Gets all available (non-blocked) points in the grid
    /// </summary>
    /// <returns>List of available transforms</returns>
    public List<Transform> GetAllAvailablePoints()
    {
        List<Transform> availablePoints = new List<Transform>();
        
        foreach (var row in _gridRows)
        {
            foreach (var point in row)
            {
                if (point != null && point.gameObject.activeInHierarchy)
                {
                    GridPoint gridPoint = point.GetComponent<GridPoint>();
                    if (gridPoint != null && !gridPoint.isBlocked)
                    {
                        availablePoints.Add(point);
                    }
                }
            }
        }
        
        return availablePoints;
    }

    /// <summary>
    /// Finds the closest available point to the given world position
    /// </summary>
    /// <param name="worldPosition">Target world position</param>
    /// <param name="chosenRow">Output parameter for the row of the chosen point</param>
    /// <returns>Transform of the closest available point, or null if none found</returns>
    public Transform GetClosestAvailablePoint(Vector3 worldPosition, out int chosenRow)
    {
        Transform closestPoint = null;
        float minDistance = float.MaxValue;
        chosenRow = -1;

        int currentRowInQueue = 0;
        foreach (var row in _gridRows)
        {
            foreach (var point in row)
            {
                if (point == null || !point.gameObject.activeInHierarchy) continue;

                GridPoint gridPoint = point.GetComponent<GridPoint>();
                if (gridPoint == null || gridPoint.isBlocked) continue;

                float dist = Vector3.Distance(point.position, worldPosition);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestPoint = point;
                    chosenRow = _rowNum - _gridRows.Count + currentRowInQueue + 1;
                }
            }
            currentRowInQueue++;
        }

        return closestPoint;
    }

    /// <summary>
    /// Overload without row output parameter
    /// </summary>
    public Transform GetClosestAvailablePoint(Vector3 worldPosition)
    {
        return GetClosestAvailablePoint(worldPosition, out var _);
    }

    /// <summary>
    /// Gets the row and column indices of a given point
    /// </summary>
    /// <param name="point">The transform to find indices for</param>
    /// <param name="rowIndex">Output row index</param>
    /// <param name="columnIndex">Output column index</param>
    /// <returns>True if point was found, false otherwise</returns>
    public bool GetPointIndices(Transform point, out int rowIndex, out int columnIndex)
    {
        rowIndex = -1;
        columnIndex = -1;
        
        if (point == null) return false;
        
        var rows = _gridRows.ToArray();
        for (int i = 0; i < rows.Length; i++)
        {
            for (int j = 0; j < rows[i].Length; j++)
            {
                if (rows[i][j] == point)
                {
                    rowIndex = i;
                    columnIndex = j;
                    return true;
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// Gets points within a certain radius of the given position
    /// </summary>
    /// <param name="worldPosition">Center position</param>
    /// <param name="radius">Search radius</param>
    /// <param name="onlyAvailable">If true, only returns non-blocked points</param>
    /// <returns>List of points within radius, sorted by distance</returns>
    public List<Transform> GetPointsInRadius(Vector3 worldPosition, float radius, bool onlyAvailable = true)
    {
        List<Transform> pointsInRadius = new List<Transform>();
        
        foreach (var row in _gridRows)
        {
            foreach (var point in row)
            {
                if (point == null || !point.gameObject.activeInHierarchy) continue;
            
                if (onlyAvailable)
                {
                    GridPoint gridPoint = point.GetComponent<GridPoint>();
                    if (gridPoint == null || gridPoint.isBlocked) continue;
                }
            
                float distance = Vector3.Distance(point.position, worldPosition);
                if (distance <= radius)
                {
                    pointsInRadius.Add(point);
                }
            }
        }
        
        // Sort by distance
        pointsInRadius.Sort((a, b) => 
            Vector3.Distance(worldPosition, a.position).CompareTo(
                Vector3.Distance(worldPosition, b.position)));
        
        return pointsInRadius;
    }

    /// <summary>
    /// Gets neighboring points around a specific grid position
    /// </summary>
    /// <param name="rowIndex">Row index of center point</param>
    /// <param name="columnIndex">Column index of center point</param>
    /// <param name="radius">How many grid positions away to search</param>
    /// <param name="onlyAvailable">If true, only returns non-blocked points</param>
    /// <returns>List of neighboring points</returns>
    public List<Transform> GetNeighboringPoints(int rowIndex, int columnIndex, int radius = 1, bool onlyAvailable = true)
    {
        List<Transform> neighbors = new List<Transform>();
        
        for (int r = rowIndex - radius; r <= rowIndex + radius; r++)
        {
            for (int c = columnIndex - radius; c <= columnIndex + radius; c++)
            {
                // Skip the center point itself
                if (r == rowIndex && c == columnIndex) continue;
            
                if (IsValidPosition(r, c))
                {
                    Transform point = GetPointAt(r, c);
                    if (point != null && point.gameObject.activeInHierarchy)
                    {
                        if (onlyAvailable)
                        {
                            GridPoint gridPoint = point.GetComponent<GridPoint>();
                            if (gridPoint != null && !gridPoint.isBlocked)
                            {
                                neighbors.Add(point);
                            }
                        }
                        else
                        {
                            neighbors.Add(point);
                        }
                    }
                }
            }
        }
        
        return neighbors;
    }
}