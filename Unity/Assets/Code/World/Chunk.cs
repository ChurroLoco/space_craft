using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshCollider))]
[RequireComponent (typeof (MeshRenderer))]
public class Chunk : MonoBehaviour
{	
	public const int VERTS_PER_BLOCK = 24;
	
	public const int WIDTH = 8;
	public const int HEIGHT = 8;	
	public const int DEPTH = 8;
	public long id;
	public MeshCollider meshCollider;
	public MeshFilter meshFilter;

	public Chuck chuck;
	
	public Block[,,] blocks = new Block[WIDTH, HEIGHT, DEPTH];
		
	public void init(Chuck chuck)
	{	
		this.chuck = chuck;
		SinBlock(0.3f, 0.6f, 0.7f, 0.3f);
		//FillBlocks();
		GenerateGeometry();
	}

	public Block GetBlockAtIndex(int x, int y, int z)
	{
		if (x < 0 || x >= WIDTH ||
		    y < 0 || y >= HEIGHT ||
		    z < 0 || z >= DEPTH)
		{
			return null;
		}
		else
		{
			return blocks[x, y, z];
		}
	}
	
	private void FillBlocks()
	{
		for (int x = 0; x < WIDTH; x++)
		{
			for (int y = 0; y < HEIGHT; y++)
			{
				for (int z = 0; z < DEPTH; z++)
				{
					blocks[x, y, z] = new Block(BlockType.All[1], this, x, y, z);
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

					if (normalizedheight <= cutOff)
					{
						blocks[x, y, z] = new Block(BlockType.All[1], this, x, y, z);
					}
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
					if (Random.Range(0, 2) == 1)
					{
						blocks[x, y, z] = new Block(BlockType.All[1], this, x, y, z);
					}
					else
					{
						blocks[x, y, z] = null;
					}
				}
			}
		}
	}
	
	public bool IsBlockEmpty(int x, int y, int z)
	{	
		return (x < 0 || x >= WIDTH ||
				y < 0 || y >= HEIGHT ||
				z < 0 || z >= DEPTH ||
				blocks[x, y, z] == null);
	}
	
	public bool IsBlockFilled(int x, int y, int z)
	{	
		return (x >= 0 && x < WIDTH &&
				y >= 0 && y < HEIGHT &&
				z >= 0 && z < DEPTH &&
				blocks[x, y, z] != null);
	}
		
	public void GenerateGeometry()
	{
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
						Block block = blocks[x,y,z];
						List<Vector3> verts = block.getVerts();
						newChunkVertices.AddRange(verts);
						newChunkNormals.AddRange(block.getNormals());
						newChunkUVs.AddRange(block.getUVs());
						newChunkTrianlges.AddRange(block.getIndices(newChunkVertices.Count - verts.Count));
						
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

	public void SetBlock(Vector3 position, int typeId, Chuck chuck)
	{
		Block block = TerrainControllerScript.GetBlockAt(position, chuck);
		if (typeId > 0 && block == null)
		{
			blocks[(int)position.x % Chunk.WIDTH, (int)position.y % Chunk.HEIGHT, (int)position.z % Chunk.DEPTH] = new Block(BlockType.All[typeId], this, (int)position.x % Chunk.WIDTH, (int)position.y % Chunk.HEIGHT, (int)position.z % Chunk.DEPTH);
			GenerateGeometry();
		}
		else if (typeId == 0 && block != null)
		{
			blocks[(int)position.x % Chunk.WIDTH, (int)position.y % Chunk.HEIGHT, (int)position.z % Chunk.DEPTH] = null;
			GenerateGeometry();
		}
	}
}