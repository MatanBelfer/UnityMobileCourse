using UnityEngine;

public static class Extensions
{
	public static string GetName(this ControlScheme scheme)
	{
		switch (scheme)
		{
			case ControlScheme.DragAndDrop:
				return "Drag and Drop";
			case ControlScheme.TapTap:
				return "Tap Tap";
			default:
				return "Unknown";
		}
	}

	public static string GetName(this Difficulty difficulty)
	{
		switch (difficulty)
		{
			case Difficulty.Easy:
				return "Chill";
			case Difficulty.Hard:
				return "Challenging!";
			default:
				return "Unknown";
		}
	}
	
	public static string GetName(this DataCollectionPermission permission)
	{
		switch (permission)
		{
			case DataCollectionPermission.Allowed:
				return "Not Allowed (data deleted)";
			case DataCollectionPermission.NotAllowed:
				return "Allowed";
			default:
				return "Unknown";
		}
	}
}
