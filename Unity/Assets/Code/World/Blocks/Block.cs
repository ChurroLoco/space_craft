using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block
{
	// This is a block.
	// Blocks are part of the terrain and always need to be within a Chunk.
	// They have an int ID for each Unique block type. 0 is Empty Space. We'll have an enum for this somewhere.
	// No two blocks can take up the same space. 
	// No block can take up more than one space. 
	// And remember. Everyblock equal, everyblock loved. 

	// The ID identifier of the block.
	int id;

	// The chunk that this block belongs to.
	BlockChunkScript chunk;

	// Placememnt of the block relative to it's chunk.
	int xIndex;
	int yIndex;
	int zIndex;


	// The layer this block resides on. 
	// Layers would be for things like passable blocks, non-passable blocks, semi-passable blocks, etc. 
	// This is for raycasting and other stuff.
	int Layer;


	// Is this block translucent?
	bool occludes;

	// Do we include this block's geometry in the physics mesh of it's chunk?
	bool isPhysical;

	// Do we include this block's geometry in the visual mesh of it's chunk?
	bool isVisible;

	// Time in seconds it takes to punch the block to death.
	int health;

	// Is this block breakable by normal means?
	bool isBreakable;

	// Eventually we'll probably want a way for each block to return their own geometry, by face if possible. 
	// For now, everything is just a cube.

	// Constructor function.
	public Block(BlockChunkScript chunk, int blockID, int x, int y, int z)
	{
		this.chunk = chunk;
		this.id = blockID;

		this.xIndex = x;
		this.yIndex = y;
		this.zIndex = z;

		clone();
	}

	// Clones block specific attributes from a data structure somewhere.
	private void clone()
	{

	}

	public void destroy(int toolID)
	{
		// Do any finishing up of stuff. 
		// Drop anything that this drops. 
		// Destroy the block.
	}

	// Returns the triangle indices for a face. 
	public virtual List<int> getIndices(int startIndex)
	{
		List <int> indices = new List<int>();

		if (chunk.IsBlockEmpty(xIndex, yIndex, zIndex - 1))
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

		if (chunk.IsBlockEmpty(xIndex, yIndex + 1, zIndex))
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

		if (chunk.IsBlockEmpty(xIndex, yIndex, zIndex + 1))
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
		 
		if (chunk.IsBlockEmpty(xIndex, yIndex - 1, zIndex))
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

		if (chunk.IsBlockEmpty(xIndex - 1, yIndex, zIndex))
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

		if (chunk.IsBlockEmpty(xIndex + 1, yIndex, zIndex))
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

		if (chunk.IsBlockEmpty(xIndex, yIndex, zIndex - 1))
		{
			uvs.AddRange(new Vector2[]{
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(1, 1),
				new Vector3(1, 0)
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex, yIndex + 1, zIndex))
		{
			uvs.AddRange(new Vector2[]{
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(1, 1),
				new Vector3(1, 0)
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex, yIndex, zIndex + 1))
		{
			uvs.AddRange(new Vector2[]{
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(1, 1),
				new Vector3(1, 0)
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex, yIndex - 1, zIndex))
		{
			uvs.AddRange(new Vector2[]{
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(1, 1),
				new Vector3(1, 0)
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex - 1, yIndex, zIndex))
		{
			uvs.AddRange(new Vector2[]{
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(1, 1),
				new Vector3(1, 0)
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex + 1, yIndex, zIndex))
		{
			uvs.AddRange(new Vector2[]{
				new Vector2(0, 0),
				new Vector2(0, 1),
				new Vector2(1, 1),
				new Vector3(1, 0)
			});
		}
		return uvs;
	}

	// Returns the Normals for the verts of a face.
	public virtual List<Vector3> getNormals()
	{
		List<Vector3> normals = new List<Vector3>();

		if (chunk.IsBlockEmpty(xIndex, yIndex, zIndex - 1))
		{
			normals.AddRange(new Vector3[]{
				Vector3.back, 
				Vector3.back, 
				Vector3.back, 
				Vector3.back
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex, yIndex + 1, zIndex))
		{
			normals.AddRange(new Vector3[]{
				Vector3.up, 
				Vector3.up, 
				Vector3.up, 
				Vector3.up
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex, yIndex, zIndex + 1))
		{
			normals.AddRange(new Vector3[]{
				Vector3.forward, 
				Vector3.forward, 
				Vector3.forward, 
				Vector3.forward
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex, yIndex - 1, zIndex))
		{
			normals.AddRange(new Vector3[]{
				Vector3.down, 
				Vector3.down, 
				Vector3.down, 
				Vector3.down
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex - 1, yIndex, zIndex))
		{
			normals.AddRange(new Vector3[]{
				Vector3.left, 
				Vector3.left, 
				Vector3.left, 
				Vector3.left
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex + 1, yIndex, zIndex))
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
		if (chunk.IsBlockEmpty(xIndex, yIndex, zIndex - 1))
		{
			verts.AddRange(new Vector3[]{
				new Vector3(xIndex, yIndex, zIndex),
				new Vector3(xIndex, yIndex + 1, zIndex),
				new Vector3(xIndex + 1, yIndex + 1, zIndex),
				new Vector3(xIndex + 1, yIndex, zIndex)
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex, yIndex + 1, zIndex))
		{
			verts.AddRange(new Vector3[]{
				new Vector3(xIndex, yIndex + 1, zIndex),
				new Vector3(xIndex, yIndex + 1, zIndex + 1),
				new Vector3(xIndex + 1, yIndex + 1, zIndex + 1),
				new Vector3(xIndex + 1, yIndex + 1, zIndex)
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex, yIndex, zIndex + 1))
		{
			verts.AddRange(new Vector3[]{
				new Vector3(xIndex + 1, yIndex, zIndex + 1),
				new Vector3(xIndex + 1, yIndex + 1, zIndex + 1),
				new Vector3(xIndex, yIndex + 1, zIndex + 1),
				new Vector3(xIndex, yIndex, zIndex + 1)
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex, yIndex - 1, zIndex))
		{
			verts.AddRange(new Vector3[]{
				new Vector3(xIndex, yIndex, zIndex + 1),
				new Vector3(xIndex, yIndex, zIndex),
				new Vector3(xIndex + 1, yIndex, zIndex),
				new Vector3(xIndex + 1, yIndex, zIndex + 1)
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex - 1, yIndex, zIndex))
		{
			verts.AddRange(new Vector3[]{
				new Vector3(xIndex, yIndex, zIndex + 1),
				new Vector3(xIndex, yIndex + 1, zIndex + 1),
				new Vector3(xIndex, yIndex + 1, zIndex),
				new Vector3(xIndex, yIndex, zIndex)
			});
		}
		
		if (chunk.IsBlockEmpty(xIndex + 1, yIndex, zIndex))
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
