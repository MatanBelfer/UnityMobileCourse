using UnityEngine;

[CreateAssetMenu(fileName = "UtilityObject", menuName = "Scriptable Objects/UtilityObject")]
public class UtilityObject : ScriptableObject
{
    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
