using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

	public int unitDimension;						// Dimensions of each side of the cube

	public int width;
	public int length;
	public int height;

	private GameObject[,,] mapTiles;				// Array to hold map tiles

	public GameObject[] floorMaterials;				// Material to make the floor out of
	public GameObject[] wallMaterials;				// Material to make the walls out of
	public GameObject[] doorMaterials;				// Material to help with doorways

	private int entryPoint;						// Where to put entry door
	private int exitPoint;						// Where to put exit door

	public float meshOffset = 0.5f;					// Mesh offset to calculate where to render objects

    // Start is called before the first frame update
    void Start()
    {
		// Max out at a 50x50x10 cube
		width = Mathf.Min(width, 50);
		height = Mathf.Min(height, 10);
		length = Mathf.Min(length, 50);

		mapTiles = new GameObject[length, width, height]; // Set up world matrix

		BuildLevelStructure();
		AddDoors();
		AddExitDoor();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

	// Build floor and walls of level
	void BuildLevelStructure() {
		
		for (int x = 0; x < width; x++) { // rows

			for (int z = 0; z < length; z++) { // columns

				for (int y = 0; y < height; y++) { // layers

					bool isFloor = y == 0; // Floor tile
					bool isWall = (x == 0 || z == 0) && y != 0; // Wall tile

					GameObject material = isFloor ? floorMaterials[SelectRandom(floorMaterials)] : wallMaterials[SelectRandom(wallMaterials)];

					if (isFloor || isWall) {
						GameObject tile = Instantiate(material, new Vector3(x, y, z), Quaternion.Euler(Vector3.zero));
						mapTiles[x, z, y] = tile; // Add the tile to our world
					} 
				}
			}
		}
	}

	void AddDoors() {

		// Randomly select a door model to use
		GameObject doorWay = doorMaterials[SelectRandom(doorMaterials)];

		// Track dimensions of the object
		// Each max - min + 1 pair should calculate the dimensional size
		float xMin = 0f;
		float xMax = 0f;
		float yMin = 0f;
		float yMax = 0f;
		float zMin = 0f;
		float zMax = 0f;

		bool hasSet = false;

		// Pull out the dimensions of the doorway
		for (int i = 0; i < doorWay.transform.childCount; i++)
		{
			Transform layer = doorWay.transform.GetChild(i);

			for (int j = 0; j < layer.transform.childCount; j++)
			{

				// Gather dimensions of the door

				// Clear the tiles between those dimensions

				// Add in the door
				Transform child = layer.transform.GetChild(j); // Grab the child block
				float xPos = child.transform.position.x + meshOffset;
				float yPos = child.transform.position.y;
				float zPos = child.transform.position.z + meshOffset;

				if (!hasSet || xPos > xMax) xMax = xPos;
				if (!hasSet || xPos < xMin) xMin = xPos;
				if (!hasSet || yPos > yMax) yMax = yPos;
				if (!hasSet || yPos < yMin) yMin = yPos;
				if (!hasSet || zPos > zMax) zMax = zPos;
				if (!hasSet || zPos < zMin) zMin = zPos;

				if (!hasSet)
				{
					hasSet = true;
				}
			}
		}

		// Dimensions of the doorway
		float xSize = xMax - xMin + 1;
		float ySize = yMax - yMin + 1;
		float zSize = zMax - zMin + 1;

		// Randomly select an entry (x) and exit (z) point to work from
		entryPoint = Random.Range(0, width - (int)xSize);

		// Delete tiles that go where the doorway will be rendered


		for (int i = 0; i < xSize; i++)  // x axis
		{
			for (int j = 0; j < zSize; j++) // z axis
			{
				for (int k = 1; k < ySize + 1; k++) // y axis, add one to offset the ground
				{
					Destroy(mapTiles[(int)i + entryPoint, (int)j, (int)k]);
				}

			}
		}

		// Place the doorway in the world
		for (int i = 0; i < doorWay.transform.childCount; i++)
		{
			Transform layer = doorWay.transform.GetChild(i);

			for (int j = 0; j < layer.transform.childCount; j++)
			{

				// Gather dimensions of the door

				// Clear the tiles between those dimensions

				// Add in the door
				Transform child = layer.transform.GetChild(j); // Grab the child block
				float xPos = child.transform.position.x + meshOffset;
				float yPos = child.transform.position.y + 1; // Add one to offset the ground
				float zPos = child.transform.position.z + meshOffset;
				
				GameObject doorTile = Instantiate(
					child.gameObject,
					new Vector3(xPos - 2 + entryPoint, yPos, zPos - 1),
					Quaternion.Euler(
						new Vector3(
							child.gameObject.transform.eulerAngles.x,
							child.gameObject.transform.eulerAngles.y,
							child.gameObject.transform.eulerAngles.z
				)));
				
				mapTiles[(int)xPos + entryPoint, (int)zPos, (int)yPos] = child.gameObject;
			}
		}
	}

	void AddExitDoor()
	{
		// Randomly select a door model to use
		GameObject doorWay = doorMaterials[SelectRandom(doorMaterials)];

		// Track dimensions of the object
		// Each max - min + 1 pair should calculate the dimensional size
		float xMin = 0f;
		float xMax = 0f;
		float yMin = 0f;
		float yMax = 0f;
		float zMin = 0f;
		float zMax = 0f;

		bool hasSet = false;

		// Pull out the dimensions of the doorway
		for (int i = 0; i < doorWay.transform.childCount; i++)
		{
			Transform layer = doorWay.transform.GetChild(i);

			for (int j = 0; j < layer.transform.childCount; j++)
			{

				// Gather dimensions of the door

				// Clear the tiles between those dimensions

				// Add in the door
				Transform child = layer.transform.GetChild(j); // Grab the child block
				float xPos = child.transform.position.x + meshOffset;
				float yPos = child.transform.position.y;
				float zPos = child.transform.position.z + meshOffset;

				if (!hasSet || xPos > xMax) xMax = xPos;
				if (!hasSet || xPos < xMin) xMin = xPos;
				if (!hasSet || yPos > yMax) yMax = yPos;
				if (!hasSet || yPos < yMin) yMin = yPos;
				if (!hasSet || zPos > zMax) zMax = zPos;
				if (!hasSet || zPos < zMin) zMin = zPos;

				if (!hasSet)
				{
					hasSet = true;
				}
			}
		}

		// Dimensions of the doorway
		float xSize = zMax - zMin + 1;
		float ySize = yMax - yMin + 1;
		float zSize = xMax - xMin + 1;

		// Randomly select an entry (x) and exit (z) point to work from
		exitPoint = Random.Range(0, length - (int)xSize);

		// Delete tiles that go where the doorway will be rendered


		for (int i = 0; i < xSize; i++)  // x axis
		{
			for (int j = 0; j < zSize; j++) // z axis
			{
				for (int k = 1; k < ySize + 1; k++) // y axis, add one to offset the ground
				{
					// Destroy(mapTiles[(int)i , (int)j + exitPoint, (int)k]);
				}

			}
		}

		// Place the doorway in the world
		for (int i = 0; i < doorWay.transform.childCount; i++)
		{
			Transform layer = doorWay.transform.GetChild(i);

			for (int j = 0; j < layer.transform.childCount; j++)
			{

				// Gather dimensions of the door

				// Clear the tiles between those dimensions

				// Add in the door
				Transform child = layer.transform.GetChild(j); // Grab the child block
				float xPos = child.transform.position.z + meshOffset;
				float yPos = child.transform.position.y + 1; // Add one to offset the ground
				float zPos = child.transform.position.x + meshOffset;
				
				GameObject doorTile = Instantiate(
					child.gameObject,
					new Vector3(xPos - 1, yPos, zPos - 1 + entryPoint),
					Quaternion.Euler(
						new Vector3(
							child.gameObject.transform.eulerAngles.z,
							child.gameObject.transform.eulerAngles.y,
							child.gameObject.transform.eulerAngles.x
				)));
				
				mapTiles[(int)xPos, (int)zPos + exitPoint, (int)yPos] = child.gameObject;
			}
		}
	}

	// Select a random material from the list
	private int SelectRandom(GameObject[] materials)
	{
		return Random.Range(0, materials.Length);
	}
}
