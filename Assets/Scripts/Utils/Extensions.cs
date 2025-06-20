using UnityEngine;

namespace RubberClimber
{
	static public class Extensions
	{
		public static string ToString(this ControlScheme scheme)
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
	}
}
