using System;
using UnityEngine;

public class TubeMesh : MonoBehaviour
{
	[SerializeField] private MeshFilter meshFilter;
    void Start()
    {
        meshFilter.mesh = Tube();
    }
    
	private Mesh Tube()
	{
		//sets the mesh in meshFilter as a snake with radius 1 from anchoor1 to anchor2
		
		Mesh mesh = new Mesh();
		
		int numCenters = 50; //points on the curve
		// float3[] centers = new float3[numCenters];
		// float3[] forwards = new float3[numCenters];

		int numVertsPerCenter = 50;
		int numVerts = numVertsPerCenter * numCenters;
		Vector3[] vertices = new Vector3[numVerts];
		Vector2[] uvs = new Vector2[numVerts];
		Vector3[] normals = new Vector3[numVerts];

		Vector3[] rays = new Vector3[numVertsPerCenter];
		for (int i = 0; i < numVertsPerCenter; i++)
		{
			rays[i] = UnitVectorByAngle(Vector3.forward, Vector3.up,
				i * (2 * (float)Math.PI) / (numVertsPerCenter - 1));
		}
		
		for (int i = 0; i < numCenters; i++)
		{
			// splineContainer.Evaluate(centerPositions[i], out centers[i], out forwards[i], out _);
			// Matrix4x4 rotation = Matrix4x4.Rotate(Quaternion.FromToRotation(Vector3.forward, (Vector3)forwards[i]));

			Vector3 center = Vector3.forward * (i * 1f / (numCenters - 1) - 0.5f);
			
			for (int j = 0; j < numVertsPerCenter; j++)
			{
				int vertInd = i * numVertsPerCenter + j;
				vertices[vertInd] = center + rays[j];
				uvs[vertInd] = new Vector2((float)j / (numVertsPerCenter - 1), (float)i / (numCenters - 1));
				normals[vertInd] = rays[j];
			}
		}

		int[] triangles = new int[(numVerts - numVertsPerCenter) * 6];
		for (int i = 0; i < numCenters - 1; i++)
		{
			for (int j = 0; j < numVertsPerCenter - 1; j++)
			{
				int startInd = i * numVertsPerCenter * 6 + j * 6;
				//int nextVert = (j < numVertsPerCenter - 1) ? j + 1 : 0;
				triangles[startInd]     = i * numVertsPerCenter + j;
				triangles[startInd + 1] = (i + 1) * numVertsPerCenter + j;
				triangles[startInd + 2] = i * numVertsPerCenter + j + 1;
				triangles[startInd + 3] = (i + 1) * numVertsPerCenter + j;
				triangles[startInd + 4] = (i + 1) * numVertsPerCenter + j + 1;
				triangles[startInd + 5] = i * numVertsPerCenter + j + 1;
			}
		}
		
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		mesh.normals = normals;

		return mesh;
	}
    
	private Vector3 UnitVectorByAngle(Vector3 axis, Vector3 forward, float angle)
	{
		//returns a vector that is forward rotated by angle radians about axis
		axis = axis.normalized;
		forward = forward.normalized;
		Vector3 side = -Vector3.Cross(axis, forward);

		return forward * (float)Math.Cos(angle) + side * (float)Math.Sin(angle);
	}
}
