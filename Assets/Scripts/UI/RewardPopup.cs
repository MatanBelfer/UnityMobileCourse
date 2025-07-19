using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardPopup : MonoBehaviour
{
    public int rewardAmount;
    public int streak;
    
    [SerializeField] private TMP_Text rewardText;

    private Dictionary<int, string> intNames = new()
    {
        {1, "One"},
        {2, "Two"},
        {3, "Three"},
        {4, "Four"},
        {5, "Five"},
        {6, "Six"},
        {7, "Seven"}
    };
    
    public void OpenPopup()
    {
        if (streak < 2)
        {
            rewardText.text = $"You got {rewardAmount} coins!";
        }
        else
        {
            rewardText.text = $"You got {rewardAmount} coins!\n{intNames[streak]} day streak!";
        }
        
        gameObject.SetActive(true);
    }
}
