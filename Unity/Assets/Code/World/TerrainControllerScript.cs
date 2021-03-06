using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainControllerScript : MonoBehaviour
{
	public const int WIDTH = 999;
	public const int HEIGHT = 8; // 256 blocks high.
	public const int DEPTH = 999;

	const string SECTOR_PREFAB_PATH = "Prefabs/Sector";

	// The list of all fully loaded Sectors.
	public List<Sector> activeSectors = new List<Sector>();

	// The list of Sectors that have been instantiated but not generated.
	public List<Sector> SectorsToGenerate = new List<Sector>();

	public Sector loadingSector = null;

	private const int ACTIVE_SECTOR_LIMIT = 40;

	public static TerrainControllerScript instance { get; private set; }

	void Start()
	{
		instance = this;
		Debug.Log("Logging sector data to: " + Application.persistentDataPath + "/secdata");
	}

	void Update()
	{
		// Load Sectors that are Waiting.
		if (loadingSector == null && SectorsToGenerate.Count > 0)
		{
			// Generate or Unload the top sector in the list.
			loadingSector = SectorsToGenerate[0];
			SectorsToGenerate.RemoveAt(0);
			if (loadingSector.canBeUnloaded)
			{
				GameObject.Destroy(loadingSector.gameObject);
				loadingSector = null;
			}
			else
			{
				StartCoroutine(loadingSector.Generate());
			}
			
		}

		Sector sectorToUnload = null;

		// Handle Active Sector Logic.
		for (int i = 0; i < activeSectors.Count; i++)
		{
			Sector sector = activeSectors[i];
			sector.ActiveUpdate();

			// Handle Unloading Logic.
			if (sectorToUnload == null && sector.canBeUnloaded && activeSectors.Count > ACTIVE_SECTOR_LIMIT)
			{
				sectorToUnload = sector;
			}
		}

		// Unload all waiting sectors and any loaded sectors in their column that can also be removed.
		if (sectorToUnload != null)
		{
			int x = sectorToUnload.xIndex;
			int z = sectorToUnload.zIndex;
			for (int i = 0; i < activeSectors.Count; i++)
			{
				Sector sector = activeSectors[i];

				if (sector.canBeUnloaded && sector.xIndex == x && sector.zIndex == z)
				{
					if (sector.dirty)
					{
						sector.Save();
					}

					activeSectors.Remove(sector);
					GameObject.Destroy(sector.gameObject);
					i--;
				}
			}
		}
	}

	// Loads a sector and puts it on a list to generate when ready.
	public static void LoadSector(int x, int y, int z)
	{
		bool alreadyExists = false;
		if (x >= 0 && x <= WIDTH &&
		    y >= 0 && y <= HEIGHT &&
		    z >= 0 && z <= DEPTH)
		{
			foreach (Sector sector in instance.activeSectors)
			{
				if (sector.xIndex == x  && sector.yIndex == y && sector.zIndex == z)
				{
					// The sector exists, ensure it isn't unloaded because we need it now.
					sector.canBeUnloaded = false;
					alreadyExists = true;
					break;
				}
			}
			if (!alreadyExists)
			{
				foreach (Sector sector in instance.SectorsToGenerate)
				{
					if (sector.xIndex == x  && sector.yIndex == y && sector.zIndex == z)
					{
						// The sector exists, ensure it isn't unloaded because we need it now.
						sector.canBeUnloaded = false;
						alreadyExists = true;
						break;
					}
				}

				if (!alreadyExists)
				{
					if (instance.loadingSector != null && instance.loadingSector.xIndex == x && instance.loadingSector.yIndex == y && instance.loadingSector.zIndex == z)
					{
						// Ensure our new sector does not unload once it is finished generating.
						instance.loadingSector.canBeUnloaded = false;
					}
					else
					{
						/// The new sector does not exist anywhere. Create it.
						Sector sector = instance.InstantiateSector(x, y, z);
						if (sector != null)
						{
							instance.SectorsToGenerate.Add(sector);
						}
					}
				}
			}
		}
	}

	// Saves the sector's data and removes the sector from any lists, deleteing it. 
	public static void UnloadSector(int x, int y, int z)
	{
		if (x >= 0 && x <= WIDTH &&
		    y >= 0 && y <= HEIGHT &&
		    z >= 0 && z <= DEPTH)
			{
			Sector sectorToRemove = null;

			// Check active sectors.
			foreach (Sector sector in instance.activeSectors)
			{
				if (sector.xIndex == x  && sector.yIndex == y && sector.zIndex == z)
				{
					sectorToRemove = sector;
					break;
				}
			}
			if (sectorToRemove == null)
			{
				// Check Waiting Sectors.
				foreach (Sector sector in instance.SectorsToGenerate)
				{
					if (sector.xIndex == x  && sector.yIndex == y && sector.zIndex == z)
					{
						sectorToRemove = sector;
						break;
					}
				}
			}
			if (instance.loadingSector.xIndex == x && instance.loadingSector.yIndex == y && instance.loadingSector.zIndex == z)
			{
				sectorToRemove = instance.loadingSector;
			}
			if (sectorToRemove != null)
			{
				sectorToRemove.canBeUnloaded = true;
			}
		}
	}

	// Loads and instantiates a sector gameobject and adds it to a queue to populate the chunks.
	private Sector InstantiateSector(int x, int y, int z)
	{
		Object resource = Resources.Load(SECTOR_PREFAB_PATH);
		Sector SectorScript = null;
		if (resource != null)
		{
			GameObject SectorObject = Instantiate(resource) as GameObject;
			if (SectorObject != null)
			{
				SectorObject.transform.position = new Vector3(x * Sector.WIDTH * Chunk.WIDTH, y * Sector.HEIGHT * Chunk.HEIGHT, z * Sector.DEPTH * Chunk.DEPTH);
				SectorObject.name = string.Format("Sector [{0},{1},{2}]", x, y, z);
				SectorObject.transform.parent = this.transform;
				SectorScript = SectorObject.GetComponent<Sector>();
				SectorScript.init(x, y, z);
			}
			else
			{
				Debug.LogError(string.Format("Sector prefab could not be loaded from '{0}'", SECTOR_PREFAB_PATH));	

			}
		}
		return SectorScript;
	}

	public void ActivateSector(Sector sector)
	{
		if (!activeSectors.Contains(sector) && loadingSector == sector)
		{
			activeSectors.Add(sector);
		}
		else
		{
			// redundancy check.
			GameObject.Destroy(sector.gameObject);
		}
		loadingSector = null;
	}




	public static Sector GetSectorAt(Vector3 position)
	{
		Sector result = null;
		foreach (Sector sector in instance.activeSectors)
		{
			if (sector.xIndex == (int)(position.x / (Sector.WIDTH * Chunk.WIDTH)) &&
			    sector.yIndex == (int)(position.y / (Sector.HEIGHT * Chunk.HEIGHT)) &&
			    sector.zIndex ==  (int)(position.z / (Sector.DEPTH * Chunk.DEPTH)))
			{
				result = sector;
				break;
			}
		}
		return result;
	}

	private Vector3 PositionRelativeToSector(Vector3 position, Sector sector)
	{
		return new Vector3(
			position.x - (sector.xIndex * Sector.WIDTH * Chunk.WIDTH),
			position.y - (sector.yIndex * Sector.HEIGHT * Chunk.HEIGHT),
			position.z - (sector.zIndex * Sector.DEPTH * Chunk.DEPTH));
	}

	public static Chunk GetChunkAt(Vector3 position, Sector sector = null)
	{
		Chunk result = null;

		if (sector == null)
		{
			sector = GetSectorAt(position);
		}
		if (sector != null)
		{
			Vector3 relativePosition = instance.PositionRelativeToSector(position, sector);

			if (relativePosition.x > 0 && relativePosition.x <= Sector.WIDTH * Chunk.WIDTH &&
				    relativePosition.y > 0 && relativePosition.y <= Sector.HEIGHT * Chunk.HEIGHT &&
				    relativePosition.z > 0 && relativePosition.z <= Sector.DEPTH * Chunk.DEPTH)
			{
				result = sector.chunks[(int)(relativePosition.x / Chunk.WIDTH),(int)(relativePosition.y / Chunk.HEIGHT),(int)(relativePosition.z / Chunk.DEPTH)];
			}
		}

		return result;
	}

	public static Block GetBlockAt(Vector3 position, Sector sector = null)
	{
		Block result = null;
		// Get the chunk.
		Chunk chunk = GetChunkAt(position, sector);
		if (chunk != null)
		{
			// Modulo the player's position by the measure of blocks in a chunk to get their position within their chunk.
			result = chunk.blocks[(int)position.x % Chunk.WIDTH, (int)position.y % Chunk.HEIGHT, (int)position.z % Chunk.DEPTH];
		}

		return result;
	}

}