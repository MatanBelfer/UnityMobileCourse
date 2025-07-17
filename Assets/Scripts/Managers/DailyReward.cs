using System;
using System.IO;
using System.Net;
using UnityEngine;

/// <summary>
/// Gives the player daily rewards for logging in. Also keeps track of up to 7 day streak
/// </summary>
public class DailyReward : BaseManager
{
	public int testStreak;
	private string jsonPath;
	
	protected override void OnInitialize()
	{
		//
	}

	protected override void OnReset()
	{
		//
	}

	protected override void OnCleanup()
	{
		//
	}

	private void Start()
	{
		//test
		//GiveReward(testStreak);
		jsonPath = Application.persistentDataPath + "/dailyReward.json";
		
		if (IsEligibleForReward(out LastLoginAndStreak lastLoginAndStreak, out bool lostStreak))
		{
			lastLoginAndStreak.streak++;
			if (lastLoginAndStreak.streak > 7) lastLoginAndStreak.streak = 7;
			GiveReward(lastLoginAndStreak.streak);
		}
		if (lostStreak)
		{
			lastLoginAndStreak.streak = 0;
		}
		
		lastLoginAndStreak.lastLoginDays = (int)(GetCurrentGlobalTime() - DateTime.UnixEpoch).TotalDays;
		string json = JsonUtility.ToJson(lastLoginAndStreak);
		print($"saving\n{json}");
		File.WriteAllText(jsonPath, json);
	}
	
	private bool IsEligibleForReward(out LastLoginAndStreak lastLoginAndStreak, out bool lostStreak)
	{
		lostStreak = false;
		
		try
		{
			string file = File.ReadAllText(jsonPath);
			print($"loaded\n{file}");
			lastLoginAndStreak = JsonUtility.FromJson<LastLoginAndStreak>(file);
		}
		catch (FileNotFoundException)
		{
			lastLoginAndStreak = new LastLoginAndStreak();
			File.WriteAllText(jsonPath, JsonUtility.ToJson(lastLoginAndStreak));
			print("Didn't find last login time");
			return false;
		}
		catch (Exception)
		{
			Debug.LogWarning("Unexpected exception when trying to read file");
			throw;
		}
		
		int lastLoginDays = lastLoginAndStreak.lastLoginDays;
		DateTime currentTime = GetCurrentGlobalTime();
		int daysSinceLastLogin = (int)(currentTime - DateTime.UnixEpoch).TotalDays - lastLoginDays;

		if (daysSinceLastLogin < 1)
		{
			return false;
		}

		if (daysSinceLastLogin >= 2)
		{
			lostStreak = true;
			return false;
		}

		return true;
	}
	
	/// <summary>
	/// lastLoginDays is the total number of days elapsed from epoch to last login
	/// </summary>
	private class LastLoginAndStreak
	{
		public int streak = 0;
		public int lastLoginDays;
	}

	//TODO: use NTP or another external time source instead of the computers' time
	private DateTime GetCurrentGlobalTime()
	{
		return DateTime.Now;
	}

	//TODO: should give the player money and show an interactive popup that has an animation but does nothing
	private void GiveReward(int streak)
	{
		int rewardAmount = CalculateReward(streak);
		ManagersLoader.GetSceneManager<ShopManager>().AddMoney(rewardAmount);
		ManagersLoader.UI.ShowRewardPopup(streak, rewardAmount);
	}

	private int CalculateReward(int streak)
	{
		return (int)(100 * Mathf.Pow(1.5f, streak - 1)); //
	}
}
