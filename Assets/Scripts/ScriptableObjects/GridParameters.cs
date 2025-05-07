using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GridParameters", menuName = "Scriptable Objects/GridParameters")]
public class GridParameters : ScriptableObject
{
    [Range(2,15)] public int pointsPerRow;
    [Range(0.1f, 1f)] public float pointSpacing;
}
