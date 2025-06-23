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
    [Range(0, 50)] public int trapStartRow = 5; // New parameter
    public string spikePoolName = "SpikePool";
    [Range(5, 30)] public int initialSpikePoolSize = 10;
    
    
    [Header("Trap Settings")]
    public GameObject pointPrefab;
    public string pointPoolName = "Holes";
}