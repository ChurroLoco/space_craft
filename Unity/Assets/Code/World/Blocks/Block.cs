using UnityEngine;
using System.Collections;

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

	// The ID of the chunk that this block belongs to.
	int chunkID;

	// Placememnt of the block relative to it's chunk.
	int xIndex;
	int yIndex;
	int zIndex;

	// Is this block translucent?
	bool occludes;

	// Do we include this block's geometry in the physics mesh of the chunk?
	bool isPhysical;

	// Do we include this block's geometry in the visual mesh of it's chunk?
	bool isVisible;

	// The layer this block resides on. 
	// Layers would be for things like passable blocks, non-passable blocks, semi-passable blocks, etc. 
	// This is for raycasting and other stuff.
	int Layer;


	// Eventually we'll probably want a way for each block to return their own geometry, by face if possible. 
	// For now, everything is just a cube.




	// Some notes on other block types that would probably derive from this class:
	/*
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
