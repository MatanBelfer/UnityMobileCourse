using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GridParameters", menuName = "Scriptable Objects/GridParameters")]
public class GridParameters : ScriptableObject
{
    [Range(2, int.MaxValue)] public int numPtsInRow;
    [Range(0.1f, 1f)] public float pointSpacing;
}
