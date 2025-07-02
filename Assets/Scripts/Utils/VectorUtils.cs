using UnityEngine;
using Unity.Mathematics;

public static class VectorUtils
{
	public static float ProjectedPos(Vector2 projectOn, Vector2 point, float lhsSqrMag, out bool pointInFrontOfVec)
	{
		//not normalized projection of point on projectOn, lhsSqrMag is the square magnitude of the vector
		float dotProd = Vector2.Dot(projectOn, point);
		pointInFrontOfVec = dotProd >= 0 && dotProd <= lhsSqrMag;
		return dotProd;
	}
	
	public static float RelativeSideOfLine(Vector2 lineStart2End, Vector2 lineStart2Here, out bool left, out bool right, float stickage)
	{
		//returns true for left if the point is "strongly" to the left of the line (more that the stickage value)
		//same for the right. if both are false, the point is considered collinear with the line. 
		float crossProd = CrossProduct2d(lineStart2End, lineStart2Here);
		left = crossProd > stickage;
		right = crossProd < -stickage;
		return crossProd;
	}
	
	public static float RelativeSideOfLine(Vector2 lineStart2End, Vector2 lineStart2Here, out bool left)
	{
		//returns true for left if the point is to the left of the line
		float crossProd = CrossProduct2d(lineStart2End, lineStart2Here);
		left = crossProd > 0;
		return crossProd;
	}
	
	public static float CrossProduct2d(Vector2 a, Vector2 b)
	{
		return a.x * b.y - a.y * b.x;
	}

	public static bool IsBetween(this Vector2 point, Vector2 a, Vector2 b, bool aToBClockwise)
	{
		bool pointLeftOfA, pointLeftOfB, bLeftOfA;
		RelativeSideOfLine(a, point, out pointLeftOfA);
		RelativeSideOfLine(b, point, out pointLeftOfB);
		RelativeSideOfLine(a, b, out bLeftOfA);

		bool result = pointLeftOfA && !pointLeftOfB;
		if (!bLeftOfA) result = !result;
		if (aToBClockwise) result = !result;
		return result;
	}
}
