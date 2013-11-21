using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshCollider))]
[RequireComponent (typeof (MeshRenderer))]
public class BlockChunkScript : MonoBehaviour
{	
	public const int VERTS_PER_BLOCK = 24;
	
	public const int WIDTH = 8;
	public const int HEIGHT = 8;	
	public const int DEPTH = 8;
	public long id;
	public MeshCollider meshCollider;
	public MeshFilter meshFilter;
	
	public int[,,] blocks = new int[WIDTH, HEIGHT, DEPTH];
		
	void Start ()
	{	
		SinBlock(0.3f, 0.6f, 0.7f, 0.3f);
		GenerateGeometry();
	}
	
	private void FillBlocks()
	{
		for (int x = 0; x < WIDTH; x++)
		{
			for (int y = 0; y < HEIGHT; y++)
			{
				for (int z = 0; z < DEPTH; z++)
				{
					blocks[x, y, z] = 1;
				}
			}
		}
	}

	private void SinBlock(float xFreq, float zFreq, float xAmp, float zAmp)
	{
		for (int x = 0; x < WIDTH; x++)
		{
			for (int y = 0; y < HEIGHT; y++)
			{
				for (int z = 0; z < DEPTH; z++)
				{
					float xWorldPos = (transform.position.x * WIDTH) + x;
					float zWorldPos = (transform.position.z * DEPTH) + z;
					float xSin = ((Mathf.Sin(xWorldPos * xFreq) + 1)/2) * xAmp;
					float zSin = ((Mathf.Sin(zWorldPos * zFreq) + 1)/2) * zAmp;
					float cutOff = (xSin + zSin)/2;

					float blocksHeight = (transform.position.y * HEIGHT) + y;
					float normalizedheight = blocksHeight / TerrainControllerScript.TERRAIN_HEIGHT;
					blocks[x, y, z] = normalizedheight <= cutOff ? 1 : 0;
				}
			}
		}
	}
	
	private void RandomBlocks()
	{
		for (int x = 0; x < WIDTH; x++)
		{
			for (int y = 0; y < HEIGHT; y++)
			{
				for (int z = 0; z < DEPTH; z++)
				{
					blocks[x, y, z] = Random.Range(0, 2);
				}
			}
		}
	}
	
	public bool IsBlockEmpty(int x, int y, int z)
	{	
		return (x < 0 || x >= WIDTH ||
				y < 0 || y >= HEIGHT ||
				z < 0 || z >= DEPTH ||
				blocks[x, y, z] == 0);
	}
	
	public bool IsBlockFilled(int x, int y, int z)
	{	
		return (x >= 0 && x < WIDTH &&
				y >= 0 && y < HEIGHT &&
				z >= 0 && z < DEPTH &&
				blocks[x, y, z] != 0);
	}
	
	private static int[] GetFaceIndices(int startIndex)
	{
		return new int[6]{
			startIndex,
			startIndex + 1,
			startIndex + 2,
			startIndex,
			startIndex + 2,
			startIndex + 3
		};
	}
	
	private static Vector2[] GetFaceUVs()
	{
		return new Vector2[]{
			new Vector2(0, 0),
			new Vector2(0, 1),
			new Vector2(1, 1),
			new Vector3(1, 0)
		};	
	}
		
	private void GenerateGeometry()
	{
		Debug.Log("Blah blah blah");
		List<Vector3> newChunkVertices = new List<Vector3>(WIDTH * HEIGHT * DEPTH);
		List<Vector3> newChunkNormals = new List<Vector3>(WIDTH * HEIGHT * DEPTH);
		List<Vector2> newChunkUVs = new List<Vector2>(WIDTH * HEIGHT * DEPTH);
		List<int> newChunkTrianlges = new List<int>();
		
		for (int y = HEIGHT - 1; y >= 0; y--)
		{
			for (int z = 0; z < DEPTH; z++)
			{
				for (int x = 0; x < WIDTH; x++)
				{
					if (IsBlockFilled(x, y, z))
					{
						// SOUTH (FRONT)
						if (IsBlockEmpty(x, y, z - 1))
						{
							newChunkVertices.Add(new Vector3(x, y, z));
							newChunkVertices.Add(new Vector3(x, y + 1, z));
							newChunkVertices.Add(new Vector3(x + 1, y + 1, z));
							newChunkVertices.Add(new Vector3(x + 1, y, z));
							
							Vector3 faceNormal = Vector3.back;
							newChunkNormals.AddRange(new Vector3[]{faceNormal, faceNormal, faceNormal, faceNormal});
							
							newChunkUVs.AddRange(GetFaceUVs());
							newChunkTrianlges.AddRange(GetFaceIndices(newChunkVertices.Count - 4));
						}
						
						// TOP 
						if (IsBlockEmpty(x, y + 1, z))
						{
							newChunkVertices.Add(new Vector3(x, y + 1, z));
							newChunkVertices.Add(new Vector3(x, y + 1, z + 1));
							newChunkVertices.Add(new Vector3(x + 1, y + 1, z + 1));
							newChunkVertices.Add(new Vector3(x + 1, y + 1, z));
							
							Vector3 faceNormal = Vector3.up;
							newChunkNormals.AddRange(new Vector3[]{faceNormal, faceNormal, faceNormal, faceNormal});
							
							newChunkUVs.AddRange(GetFaceUVs());
							newChunkTrianlges.AddRange(GetFaceIndices(newChunkVertices.Count - 4));
						}
						
						// NORTH (BACK) 
						if (IsBlockEmpty(x, y, z + 1))
						{
							newChunkVertices.Add(new Vector3(x + 1, y, z + 1));
							newChunkVertices.Add(new Vector3(x + 1, y + 1, z + 1));
							newChunkVertices.Add(new Vector3(x, y + 1, z + 1));
							newChunkVertices.Add(new Vector3(x, y, z + 1));
							
							Vector3 faceNormal = Vector3.forward;
							newChunkNormals.AddRange(new Vector3[]{faceNormal, faceNormal, faceNormal, faceNormal});
							
							newChunkUVs.AddRange(GetFaceUVs());
							newChunkTrianlges.AddRange(GetFaceIndices(newChunkVertices.Count - 4));
						}
						
						// BOTTOM 
						if (IsBlockEmpty(x, y - 1, z))
						{
							newChunkVertices.Add(new Vector3(x, y, z + 1));
							newChunkVertices.Add(new Vector3(x, y, z));
							newChunkVertices.Add(new Vector3(x + 1, y, z));
							newChunkVertices.Add(new Vector3(x + 1, y, z + 1));
							
							Vector3 faceNormal = Vector3.down;
							newChunkNormals.AddRange(new Vector3[]{faceNormal, faceNormal, faceNormal, faceNormal});
							
							newChunkUVs.AddRange(GetFaceUVs());
							newChunkTrianlges.AddRange(GetFaceIndices(newChunkVertices.Count - 4));
						}
						
						// LEFT (WEST) 
						if (IsBlockEmpty(x - 1, y, z))
						{
							newChunkVertices.Add(new Vector3(x, y, z + 1));
							newChunkVertices.Add(new Vector3(x, y + 1, z + 1));
							newChunkVertices.Add(new Vector3(x, y + 1, z));
							newChunkVertices.Add(new Vector3(x, y, z));
							
							Vector3 faceNormal = Vector3.left;
							newChunkNormals.AddRange(new Vector3[]{faceNormal, faceNormal, faceNormal, faceNormal});
							
							newChunkUVs.AddRange(GetFaceUVs());
							newChunkTrianlges.AddRange(GetFaceIndices(newChunkVertices.Count - 4));
						}
						
						// RIGHT (EAST) 
						if (IsBlockEmpty(x + 1, y, z))
						{
							newChunkVertices.Add(new Vector3(x + 1, y, z));
							newChunkVertices.Add(new Vector3(x + 1, y + 1, z));
							newChunkVertices.Add(new Vector3(x + 1, y + 1, z + 1));
							newChunkVertices.Add(new Vector3(x + 1, y, z + 1));
							
							Vector3 faceNormal = Vector3.right;
							newChunkNormals.AddRange(new Vector3[]{faceNormal, faceNormal, faceNormal, faceNormal});
							
							newChunkUVs.AddRange(GetFaceUVs());
							newChunkTrianlges.AddRange(GetFaceIndices(newChunkVertices.Count - 4));
						}
					}
				}
			}
		}
		
		Mesh mesh = new Mesh();
		mesh.vertices = newChunkVertices.ToArray();
		mesh.normals = newChunkNormals.ToArray();
		mesh.uv = newChunkUVs.ToArray();
		mesh.triangles = newChunkTrianlges.ToArray();
		//mesh.Optimize();
			
		if (meshFilter != null)
		{
			meshFilter.mesh = mesh;
		}
		else
		{
			Debug.LogError("'meshFilter' is null.");
		}
		
		if (meshCollider != null)
		{
			meshCollider.sharedMesh = mesh;
		}
		else
		{
			Debug.LogError("'meshCollider' is null.");
		}
	}
	
}