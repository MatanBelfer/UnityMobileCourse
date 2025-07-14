using System;
using System.IO;
using System.Net;
using UnityEngine;

/// <summary>
/// Gives the player daily rewards for logging in. Also keeps track of up to 7 day streak
/// </summary>
public class DailyReward : BaseManager
{
	private LastLoginAndStreak streak;
	private string jsonPath = Application.persistentDataPath + "/dailyReward.json";
	
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
		if (IsEligibleForReward(out LastLoginAndStreak lastLoginAndStreak))
		{
			lastLoginAndStreak.streak++;
			if (lastLoginAndStreak.streak > 7) lastLoginAndStreak.streak = 7;
			lastLoginAndStreak.lastLogin = GetCurrentGlobalTime();
			File.WriteAllText(jsonPath, JsonUtility.ToJson(lastLoginAndStreak));
			GiveReward(lastLoginAndStreak.streak);
		}
	}
	
	private bool IsEligibleForReward(out LastLoginAndStreak lastLoginAndStreak)
	{
		try
		{
			string file = File.ReadAllText(jsonPath);
			lastLoginAndStreak = JsonUtility.FromJson<LastLoginAndStreak>(file);
		}
		catch (FileNotFoundException)
		{
			lastLoginAndStreak = new LastLoginAndStreak();
			File.WriteAllText(jsonPath, JsonUtility.ToJson(lastLoginAndStreak));
			return false;
		}
		catch (Exception)
		{
			Debug.LogWarning("Unexpected exception when trying to read file");
			throw;
		}
		DateTime lastLogin = lastLoginAndStreak.lastLogin;
		DateTime currentTime = GetCurrentGlobalTime();
		float timeSinceLastLogin = (float)(currentTime - lastLogin).TotalDays;

		if (timeSinceLastLogin is < 1 or >= 2)
		{
			return false;
		}

		return true;
	}

	private class LastLoginAndStreak
	{
		public int streak = 0;
		public DateTime lastLogin;
		
		public LastLoginAndStreak()
		{
			lastLogin = DateTime.Now;
		}
	}

	//TODO: use NTP or another external time source instead of the computers' time
	private DateTime GetCurrentGlobalTime()
	{
		return DateTime.Now;
	}

	//TODO: should give the player money and show an interactive popup that has an animation but does nothing
	private void GiveReward(int streak)
	{
		throw new NotImplementedException();
	}
}
