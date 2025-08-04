using UnityEngine;

[CreateAssetMenu(fileName = "GridParameters", menuName = "Scriptable Objects/GridParameters")]
public class GridParameters : ScriptableObject
{
    [Range(2, 15)] public int pointsPerRow;
    [Range(0.1f, 1f)] public float pointSpacing;
    [Range(0f, 20f)] public float gridSpeed = 0.2f;
    [Range(1, 50)] public int initialRowCount = 25;
    
    [Header("Trap Settings")]
    public GameObject spikePrefab;
    [Range(0f, 1f)] public float spikeSpawnChance = 0.2f;
    [Range(0, 50)] public int trapStartRow = 5;
    public string spikePoolName = "SpikePool";
    [Range(5, 30)] public int initialSpikePoolSize = 10;
    
    [Header("Moving Spike Trap Settings")]
    public GameObject movingSpikePrefab;
    [Range(0f, 1f)] public float movingSpikeSpawnChance = 0.1f;
    [Range(1f, 10f)] public float movingSpikeSpeed = 2f;
    [Range(1f, 5f)] public float movingSpikeRange = 3f;
    public string movingSpikePoolName = "MovingSpikePool";
    [Range(5, 20)] public int initialMovingSpikePoolSize = 5;
    
    [Header("Collectable Settings")]
    public GameObject collectablePrefab;
    [Range(0f, 1f)] public float collectableSpawnChance = 0.15f;
    [Range(1, 100)] public int collectableScoreValue = 10;
    [Range(0, 50)] public int collectableStartRow = 3;
    public string collectablePoolName = "CollectablePool";
    [Range(5, 30)] public int initialCollectablePoolSize = 15;
    
    [Header("Point Settings")]
    public GameObject pointPrefab;
    public string pointPoolName = "Holes";
}