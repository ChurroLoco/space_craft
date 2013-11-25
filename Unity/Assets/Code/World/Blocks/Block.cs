using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block
{
	// The chunk that this block belongs to.
	Chunk chunk;

	// Placememnt of the block relative to it's chunk.
	int xIndex;
	int yIndex;
	int zIndex;


	// The layer this block resides on. 
	// Layers would be for things like passable blocks, non-passable blocks, semi-passable blocks, etc. 
	// This is for raycasting and other stuff.
	int Layer;

	public BlockType type { get; private set; }

	// Constructor function.
	public Block(BlockType type, Chunk chunk, int x, int y, int z)
	{
		this.type = type;
		this.chunk = chunk;

		this.xIndex = x;
		this.yIndex = y;
		this.zIndex = z;

		clone();
	}

	// Clones block specific attributes from a data structure somewhere.
	private void clone()
	{

	}

	// Returns the triangle indices for a face. 
	public virtual List<int> getIndices(int startIndex)
	{
		List <int> indices = new List<int>();
		Block neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex, zIndex - 1);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			indices.AddRange(new int[6]{
				startIndex,
				startIndex + 1,
				startIndex + 2,
				startIndex,
				startIndex + 2,
				startIndex + 3
			});
			startIndex += 4;
		}

		neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex + 1, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			indices.AddRange(new int[6]{
				startIndex,
				startIndex + 1,
				startIndex + 2,
				startIndex,
				startIndex + 2,
				startIndex + 3
			});
			startIndex += 4;
		}

		neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex, zIndex + 1);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			indices.AddRange(new int[6]{
				startIndex,
				startIndex + 1,
				startIndex + 2,
				startIndex,
				startIndex + 2,
				startIndex + 3
			});
			startIndex += 4;
		}

		neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex - 1, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			indices.AddRange(new int[6]{
				startIndex,
				startIndex + 1,
				startIndex + 2,
				startIndex,
				startIndex + 2,
				startIndex + 3
			});
			startIndex += 4;
		}

		neighborBlock = chunk.GetBlockAtIndex(xIndex - 1, yIndex, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			indices.AddRange(new int[6]{
				startIndex,
				startIndex + 1,
				startIndex + 2,
				startIndex,
				startIndex + 2,
				startIndex + 3
			});
			startIndex += 4;
		}

		neighborBlock = chunk.GetBlockAtIndex(xIndex + 1, yIndex, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			indices.AddRange(new int[6]{
				startIndex,
				startIndex + 1,
				startIndex + 2,
				startIndex,
				startIndex + 2,
				startIndex + 3
			});
		}
		return indices;
	}

	// Returns the UV mappings for a face.
	public virtual List<Vector2> getUVs()
	{
		List<Vector2> uvs = new List<Vector2>();

		Block neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex, zIndex - 1);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			uvs.AddRange(new Vector2[]{
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(0, 0)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(0, 1)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(1, 1)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector3(1, 0))
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex + 1, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			uvs.AddRange(new Vector2[]{
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(0, 0)),
		        TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(0, 1)),
		        TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(1, 1)),
		        TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector3(1, 0))
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex, zIndex + 1);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			uvs.AddRange(new Vector2[]{
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(0, 0)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(0, 1)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(1, 1)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector3(1, 0))
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex -1, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			uvs.AddRange(new Vector2[]{
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(0, 0)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(0, 1)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(1, 1)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector3(1, 0))
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex - 1, yIndex, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			uvs.AddRange(new Vector2[]{
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(0, 0)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(0, 1)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(1, 1)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector3(1, 0))
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex + 1, yIndex, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			uvs.AddRange(new Vector2[]{
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(0, 0)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(0, 1)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector2(1, 1)),
				TerrainAtlas.UVCordFromIndex(this.type.TextureIndex, new Vector3(1, 0))
			});
		}
		return uvs;
	}

	// Returns the Normals for the verts of a face.
	public virtual List<Vector3> getNormals()
	{
		List<Vector3> normals = new List<Vector3>();

		Block neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex, zIndex - 1);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			normals.AddRange(new Vector3[]{
				Vector3.back, 
				Vector3.back, 
				Vector3.back, 
				Vector3.back
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex + 1, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			normals.AddRange(new Vector3[]{
				Vector3.up, 
				Vector3.up, 
				Vector3.up, 
				Vector3.up
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex, zIndex + 1);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			normals.AddRange(new Vector3[]{
				Vector3.forward, 
				Vector3.forward, 
				Vector3.forward, 
				Vector3.forward
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex - 1, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			normals.AddRange(new Vector3[]{
				Vector3.down, 
				Vector3.down, 
				Vector3.down, 
				Vector3.down
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex - 1, yIndex, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			normals.AddRange(new Vector3[]{
				Vector3.left, 
				Vector3.left, 
				Vector3.left, 
				Vector3.left
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex + 1, yIndex, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			normals.AddRange(new Vector3[]{
				Vector3.right, 
				Vector3.right, 
				Vector3.right, 
				Vector3.right
			});
		}
		return normals;
	}

	// Returns the verts that make up a face of the geometry.
	public virtual List<Vector3> getVerts()
	{
		List<Vector3> verts = new List<Vector3>();
		Block neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex, zIndex - 1);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			verts.AddRange(new Vector3[]{
				new Vector3(xIndex, yIndex, zIndex),
				new Vector3(xIndex, yIndex + 1, zIndex),
				new Vector3(xIndex + 1, yIndex + 1, zIndex),
				new Vector3(xIndex + 1, yIndex, zIndex)
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex + 1, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			verts.AddRange(new Vector3[]{
				new Vector3(xIndex, yIndex + 1, zIndex),
				new Vector3(xIndex, yIndex + 1, zIndex + 1),
				new Vector3(xIndex + 1, yIndex + 1, zIndex + 1),
				new Vector3(xIndex + 1, yIndex + 1, zIndex)
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex, zIndex + 1);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			verts.AddRange(new Vector3[]{
				new Vector3(xIndex + 1, yIndex, zIndex + 1),
				new Vector3(xIndex + 1, yIndex + 1, zIndex + 1),
				new Vector3(xIndex, yIndex + 1, zIndex + 1),
				new Vector3(xIndex, yIndex, zIndex + 1)
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex, yIndex - 1, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			verts.AddRange(new Vector3[]{
				new Vector3(xIndex, yIndex, zIndex + 1),
				new Vector3(xIndex, yIndex, zIndex),
				new Vector3(xIndex + 1, yIndex, zIndex),
				new Vector3(xIndex + 1, yIndex, zIndex + 1)
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex - 1, yIndex, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			verts.AddRange(new Vector3[]{
				new Vector3(xIndex, yIndex, zIndex + 1),
				new Vector3(xIndex, yIndex + 1, zIndex + 1),
				new Vector3(xIndex, yIndex + 1, zIndex),
				new Vector3(xIndex, yIndex, zIndex)
			});
		}
		
		neighborBlock = chunk.GetBlockAtIndex(xIndex + 1, yIndex, zIndex);
		if (neighborBlock == null ||
		    neighborBlock.type.OccludesNeighbors == false)
		{
			verts.AddRange(new Vector3[]{
				new Vector3(xIndex + 1, yIndex, zIndex),
				new Vector3(xIndex + 1, yIndex + 1, zIndex),
				new Vector3(xIndex + 1, yIndex + 1, zIndex + 1),
				new Vector3(xIndex + 1, yIndex, zIndex + 1)
			});
		}
		return verts;
	}




	// Some notes on other block types that would probably derive from this class:
	/*
	 * 
	 * 
	 * Non-Cubic Blocks. (Blocks that have Geometry other than a 1x1x1 cube, like stairs or stuff)
	 * ================
	 * Blocks with non-cube proportions should hold a reference to their geometry 
	 * or the algorithm to create it. 
	 * 
	 * In this case, they should also return a bool for each face which tells the chunk's mesh whether to 
	 * cull the geometry of that face if there is a nontranslucent block next to it.
	 * 
	 * 
	 * 
	 * Special visual/Physics Mesh.
	 * ===========================
	 * A mesh that has a seperate physics mesh and visual mesh.
	 * I can't think of any examples right now, but hey.
	 * 
	 * 
	 * 
	 * Animate blocks. (Like chests and the bases of pistons stuff.)
	 * ===============
	 * Blocks that can animate. These blocks will always have a static physics mesh and a seperate visual mesh which is animated.
	 * This lets us not have to rebuild a physics mesh each step thus messing with the player controller. 
	 * It also makes things much simpler when animating outside of a block's 1x1x1 space.
	 * 
	 * In the example of a piston block extending, the base block would hold the physics mesh of the base and the visual mesh of both the base and the head.
	 * The visual head would animate via translation and once it is fully extended and any other resulting stuff taken care of,
	 * the now empty space is populated with a piston head block, which has a physics mesh but no visual counterpart.
	 * 
	 * 
	*/

}
