using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{

	public int numRooms;				// Number of rooms to instantiate
	public GameObject[] RoomTemplates;	// Room prefabs
	private List<GameObject> generatedRooms;			// Keep references the rooms

	public int mapWidth;				// x axis
	public int mapLength;				// z axis
	public int mapHeight;				// y axis

    // Start is called before the first frame update
    void Start()
    {
		generatedRooms = new List<GameObject>();
        BuildRooms();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void BuildRooms()
	{
		for (int i = 0; i < numRooms; i++)
		{
			GameObject selectedRoomTemplate = RoomTemplates[Random.Range(0, RoomTemplates.Length)];

			Debug.Log(selectedRoomTemplate);

			RoomBuilder templateCollider = selectedRoomTemplate.GetComponent<RoomBuilder>();

			int spawnX = Random.Range(0, mapWidth - templateCollider.width);
			int spawnZ = Random.Range(0, mapLength - templateCollider.length);

			GameObject newRoom = Instantiate(selectedRoomTemplate, new Vector3(spawnX, 0f, spawnZ), Quaternion.Euler(Vector3.zero));

			RoomBuilder rbScript = newRoom.GetComponent<RoomBuilder>();
			rbScript.index = i;
			newRoom.name = "Room " + i;
			generatedRooms.Add(newRoom);
		}
	}
}
