using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridManager : BaseManager
{
    //grid settings
    [SerializeField] private GridParameters[] gridParametersList;
    public GridParameters gridParameters;

    [Tooltip("The distance in meters between adjacent grid points")]
    public float gridLength;

    private HashSet<Transform> activePoints = new HashSet<Transform>();

    //privates
    private bool _showDebugInfo = true; // Set to false to disable debug visualization
    private Color _gizmoTextColor = Color.yellow; // You can change the color to make it more visible

    //grid structure
    private Queue<GridPoint[]> _gridRows = new();
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


    public int GetCurrentRowPointCount()
    {
        if (gridParameters == null) return 0;

        int nextRow = _rowNum;
        return (nextRow % 2 == 0) ? gridParameters.pointsPerRow : gridParameters.pointsPerRow - 1;
    }

    public void SpawnNewRow()
    {
        if (!_isGridInitialized || gridParameters == null) return;
        SpawnRow();
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

        GridPoint[] rowPoints = new GridPoint[numPoints];
        Vector3[] positions = new Vector3[numPoints];
        GridPoint _Point;
        for (int i = 0; i < numPoints; i++)
        {
            positions[i].x = (-(numPoints - 1) * gridParameters.pointSpacing / 2) + i * gridParameters.pointSpacing;

            _Point = SpawnPoint(positions[i]).GetComponent<GridPoint>();
            // rowPoints[i] = SpawnPoint(positions[i]);
            rowPoints[i] = _Point;
            // rowPoints[i].GetComponent<GridPoint>().column = i;
            _Point.column = i;
            _Point.GridRow = _rowNum;
        }

        SpawnObjectsOnRow(rowPoints);

        _gridRows.Enqueue(rowPoints);
        _rowNum++;
    }

    private void SpawnSpike(GridPoint point)
    {
        if (gridParameters == null || ManagersLoader.Pool == null) return;
        GameObject spike = ManagersLoader.Pool.GetFromPool(gridParameters.spikePoolName);
        spike.transform.parent = transform;
        spike.transform.localPosition = point.transform.position + _rowNum * _rowDist * Vector3.up;

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


    public GridPoint GetClosestPoint(Vector2 position, out int chosenRow)
    {
        GridPoint closestPoint = null;
        float minDistance = float.MaxValue;
        chosenRow = -1;

        int currentRowInQueue = 0;
        foreach (var row in _gridRows)
        {
            foreach (var point in row)
            {
                if (point == null) continue;

                float dist = Vector3.Distance(point.transform.position, position);
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

    public GridPoint GetClosestPoint(Vector2 position)
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
                Vector3 labelPosition = rows[i][0].transform.position + Vector3.left * 0.5f;

                // Draw the row number
                Handles.color = _gizmoTextColor;
                Handles.Label(labelPosition, $"Row: {i}");

                // Optionally, draw a line connecting all points in the row
                Gizmos.color = Color.cyan;
                for (int j = 0; j < rows[i].Length - 1; j++)
                {
                    if (rows[i][j] != null && rows[i][j + 1] != null)
                    {
                        Gizmos.DrawLine(rows[i][j].transform.position, rows[i][j + 1].transform.position);
                    }
                }
            }
        }
    }
#endif

    /*
     * public methods to interact with the grid
     */

    public GridPoint GetPointAt(int rowIndex, int columnIndex)
    {
        if (!IsValidPosition(rowIndex, columnIndex))
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
            ManagersLoader.Pool.ReturnToPool(poolInterface.poolName, obj);
        }
    }

    /// <summary>
    /// Gets all available (non-blocked) points in the grid
    /// </summary>
    /// <returns>List of available transforms</returns>
    public List<GridPoint> GetAllAvailablePoints()
    {
        List<GridPoint> availablePoints = new List<GridPoint>();

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
    public GridPoint GetClosestAvailablePoint(Vector3 worldPosition, out int chosenRow)
    {
        GridPoint closestPoint = null;
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

                float dist = Vector3.Distance(point.transform.position, worldPosition);
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
    public GridPoint GetClosestAvailablePoint(Vector3 worldPosition)
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
    public List<GridPoint> GetPointsInRadius(Vector3 worldPosition, float radius, bool onlyAvailable = true)
    {
        List<GridPoint> pointsInRadius = new List<GridPoint>();

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

                float distance = Vector3.Distance(point.transform.position, worldPosition);
                if (distance <= radius)
                {
                    pointsInRadius.Add(point);
                }
            }
        }

        // Sort by distance
        pointsInRadius.Sort((a, b) =>
            Vector3.Distance(worldPosition, a.transform.position).CompareTo(
                Vector3.Distance(worldPosition, b.transform.position)));

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
    public List<GridPoint> GetNeighboringPoints(int rowIndex, int columnIndex, int radius = 1,
        bool onlyAvailable = true)
    {
        List<GridPoint> neighbors = new List<GridPoint>();

        for (int r = rowIndex - radius; r <= rowIndex + radius; r++)
        {
            for (int c = columnIndex - radius; c <= columnIndex + radius; c++)
            {
                // Skip the center point itself
                if (r == rowIndex && c == columnIndex) continue;

                if (IsValidPosition(r, c))
                {
                    GridPoint point = GetPointAt(r, c);
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


    private void SpawnCollectable(GridPoint point)
    {
        ObjectPoolManager poolManager = ManagersLoader.GetSceneManager<ObjectPoolManager>();
        GameObject collectable = ManagersLoader.Pool.GetFromPool(gridParameters.collectablePoolName);


        if (collectable != null)
        {
            collectable.transform.parent = point.transform;
            collectable.transform.position = point.transform.position + _rowNum * _rowDist * Vector3.up;

            // Set score value from parameters
            Collectable collectableComponent = collectable.GetComponent<Collectable>();
            if (collectableComponent != null)
            {
                collectableComponent.SetScoreValue(gridParameters.collectableScoreValue);
                collectableComponent.ResetCollectable();
            }
        }
    }

    private void SpawnMovingSpike(GridPoint point)
    {
        GameObject movingSpike = null;


        movingSpike = ManagersLoader.Pool.GetFromPool(gridParameters.movingSpikePoolName);


        if (movingSpike != null)
        {
            // Find the grid point at this position to parent the moving spike to it


            movingSpike.transform.parent = point.transform;
            movingSpike.transform.localPosition = point.transform.position + _rowNum * _rowDist * Vector3.up;

            // Set movement parameters
            MovingSpikeTrap movingSpikeComponent = movingSpike.GetComponent<MovingSpikeTrap>();
            if (movingSpikeComponent != null)
            {
                movingSpikeComponent.SetMovementParameters(gridParameters.movingSpikeSpeed,
                    gridParameters.movingSpikeRange);
                movingSpikeComponent.ResetSpike();
                movingSpikeComponent.ResumeMovement();
            }
        }
    }


    private void SpawnObjectsOnRow(GridPoint[] points)
    {
        // Only spawn objects after certain row thresholds
        bool canSpawnTraps = points[0].GridRow >= gridParameters.trapStartRow;
        bool canSpawnCollectables = points[0].GridRow >= gridParameters.collectableStartRow;

        foreach (var point in points)
        {
            // Random chance to spawn different objects
            float randomValue = Random.Range(0f, 1f);

            if (canSpawnTraps && randomValue < gridParameters.spikeSpawnChance)
            {
                SpawnSpike(point);
                canSpawnTraps = false;
            }
            else if (canSpawnTraps &&
                     randomValue < (gridParameters.spikeSpawnChance + gridParameters.movingSpikeSpawnChance))
            {
                SpawnMovingSpike(point);
                canSpawnTraps = false;
            }
            else if (canSpawnCollectables && randomValue < (gridParameters.spikeSpawnChance +
                                                            gridParameters.movingSpikeSpawnChance +
                                                            gridParameters.collectableSpawnChance))
            {
                SpawnCollectable(point);
                canSpawnCollectables = false;
            }
        }
    }
}