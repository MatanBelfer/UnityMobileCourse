using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GridParameters", menuName = "Scriptable Objects/GridParameters")]
public class GridParameters : ScriptableObject
{
    [Range(2, 15)] public int pointsPerRow;
    [Range(0.1f, 1f)] public float pointSpacing;
    [Range(0f, 20f)] public float gridSpeed = 0.2f;
    [Range(1, 15)] public int initialRowCount = 8;
    
    // Traps related parameters
    [Header("Trap Settings")]
    public GameObject spikePrefab;
    [Range(0f, 1f)] public float spikeSpawnChance = 0.2f;
    public string spikePoolName = "SpikePool";
    [Range(5, 30)] public int initialSpikePoolSize = 10;
}