using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBuilder : MonoBehaviour
{
    public int unitDimension;						// Dimensions of each side of the cube

	public int width;
	public int length;
	public int height;

	private GameObject[,] floorTiles;				// Array to hold map tiles
	private GameObject[,] xWallTiles;				// Array to hold x wall tiles
	private GameObject[,] zWallTiles;				// Array to hold z wall tiles

	public GameObject[] floorMaterials;				// Material to make the floor out of
	public GameObject[] wallMaterials;				// aMaterial to make the walls out of
	public GameObject[] doorMaterials;				// Material to help with doorways

	private int entryPoint;							// Where to put entry door
	private int exitPoint;							// Where to put exit door

	public float meshOffset = 0.5f;					// Mesh offset to calculate where to render objects

	public int index;
	
	void Start() {
		BoxCollider collider = GetComponent<BoxCollider>();
		collider.size = new Vector3(width, height, length);

		floorTiles = new GameObject[width, length];
		xWallTiles = new GameObject[width, height - 1];
		zWallTiles = new GameObject[length, height - 1];

		//  floor tiles
		GenerateFloor();
		GenerateXWall();
		GenerateZWall();
	}

	void GenerateFloor() {
		for (int x = 0; x < width; x++)
		{
			for (int z = 0; z < length; z++)
			{
				GameObject floorTile = Instantiate(floorMaterials[SelectRandom(floorMaterials)], new Vector3(x + this.transform.localPosition.x - (width / 2), -height / 2, z + this.transform.localPosition.z - (length / 2)), Quaternion.Euler(Vector3.zero));
				floorTile.transform.SetParent(this.gameObject.transform);
				floorTiles[x, z] = floorTile;
			}
		}
	}

	void GenerateXWall() {

		Debug.Log("X: " + width);

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height - 1; y++)
			{
				GameObject xWallTile = Instantiate(floorMaterials[SelectRandom(floorMaterials)], new Vector3(x + this.transform.localPosition.x - (width / 2), y + 1 - (height / 2), 0f + this.transform.localPosition.z - (length / 2)), Quaternion.Euler(Vector3.zero));
				xWallTile.transform.SetParent(this.gameObject.transform);
				xWallTiles[x, y] = xWallTile;
			}
		}
	}

	void GenerateZWall() {

		Debug.Log("Z: " + length);

		for (int z = 0; z < length; z++)
		{
			for (int y = 0; y < height - 1; y++)
			{
				GameObject zWallTile = Instantiate(wallMaterials[SelectRandom(wallMaterials)], new Vector3(0f + this.transform.localPosition.x - (width / 2), y + 1 - (height / 2), z + this.transform.localPosition.z - (length / 2)), Quaternion.Euler(Vector3.zero));
				zWallTile.transform.SetParent(this.gameObject.transform);
				zWallTiles[z, y] = zWallTile;
			}
		}
	}

	// Destroy any colliding rooms
	private void OnTriggerEnter(Collider other) {

		if (other.gameObject.tag == "Generated Room")
		{
			Debug.Log("Collision");
			Debug.Log(other.gameObject.tag);
			RoomBuilder thisRoom = this.gameObject.GetComponent<RoomBuilder>();
			RoomBuilder otherRoom = other.gameObject.GetComponent<RoomBuilder>();

			Debug.Log(thisRoom.index + " is colliding with " + otherRoom.index);

			if (thisRoom.index > otherRoom.index)
			{
				Destroy(this.gameObject);
			} 
		}
	}

	// Select a random material from the list
	private int SelectRandom(GameObject[] materials)
	{
		return Random.Range(0, materials.Length);
	}

}
