using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SysRandom = System.Random;
using Graphs;

public class WorldManager : MonoBehaviour
{

	enum CellType {
        None,
        Room,
        Hallway,
		Stairs
    }

	class Room {
        public BoundsInt bounds;

        public Room(Vector3Int location, Vector3Int size) {
            bounds = new BoundsInt(location, size);
        }

        public static bool Intersect(Room a, Room b) {
            return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y)
                || (a.bounds.position.z >= (b.bounds.position.z + b.bounds.size.z)) || ((a.bounds.position.z + a.bounds.size.z) <= b.bounds.position.z));
        }
    }

	public int numRooms;								// Number of rooms to instantiate
	public GameObject[] RoomTemplates;					// Room prefabs
	private List<GameObject> generatedRooms;			// Keep references the rooms

	public int mapWidth;								// x axis
	public int mapLength;								// z axis
	public int mapHeight;								// y axis

	private Delaunay3D delaunay;
	private List<Edge> edges;
	private HashSet<Prim.Edge> selectedEdges;

	SysRandom random;

	Vector3Int size;
	Grid3D<CellType> grid;

	List<Room> rooms;
	int roomCount;
    [SerializeField]
	public GameObject cubePrefab;

    // Start is called before the first frame update
    void Start()
    {
		random = new SysRandom(0);
		Vector3Int vec = new Vector3Int();
		vec.Set(mapWidth, mapHeight, mapLength);
		grid = new Grid3D<CellType>(vec, Vector3Int.zero);
		generatedRooms = new List<GameObject>();
		StartCoroutine(GenerationSequence());
    }

	// Execute these functions in order
	// Important so PlaceRooms() can handle any collisions before triangulation
	private IEnumerator GenerationSequence()
	{
		yield return StartCoroutine(PlaceRooms());
		yield return StartCoroutine(Triangulate());
		yield return StartCoroutine(CreateHallways());
		yield return StartCoroutine(PathfindHallways());
	}


    // Update is called once per frame
    void Update()
    {
    }

	private IEnumerator PlaceRooms() {
        	for (int i = 0; i < numRooms; i++)
		{
			GameObject selectedRoomTemplate = RoomTemplates[Random.Range(0, RoomTemplates.Length)];

			RoomBuilder template = selectedRoomTemplate.GetComponent<RoomBuilder>();

			int spawnX = Random.Range(0, mapWidth - template.width);
			int spawnY = Random.Range(0, mapHeight - template.height);
			int spawnZ = Random.Range(0, mapLength - template.length);

			template.ySpawn = spawnY;

			GameObject newRoom = Instantiate(selectedRoomTemplate, new Vector3(spawnX, spawnY, spawnZ), Quaternion.Euler(Vector3.zero));

			BoxCollider col = newRoom.GetComponent<BoxCollider>();

			RoomBuilder rbScript = newRoom.GetComponent<RoomBuilder>();
			rbScript.index = i;
			newRoom.name = "Room " + i;

			generatedRooms.Add(newRoom);

			Vector3Int roomCenter = new Vector3Int();
			Vector3Int.FloorToInt(newRoom.transform.position);
			
			BoundsInt roomBounds = new BoundsInt(roomCenter, rbScript.Dimensions());

			foreach (var pos in roomBounds.allPositionsWithin)
			{
				Vector3Int coords = new Vector3Int(pos.x, pos.y, pos.z);
				grid[coords] = CellType.Room;
			}
		}

		yield return null;
    }

	private IEnumerator Triangulate()
	{
		Debug.Log(generatedRooms.Count);
		List<Vertex> vertices = new List<Vertex>();

		foreach (var room in generatedRooms.ToArray()) // Copy generated rooms to array so we can modify the original generatedRooms object as we traverse
		{
			
			if (room != null)
			{
				RoomBuilder rb = room.GetComponent<RoomBuilder>();
				Bounds rbBounds = room.GetComponent<BoxCollider>().bounds;

				Debug.Log("ROOM POSITION: " + room.transform.localPosition.ToString());
				Vertex vertex = new Vertex<RoomBuilder>((Vector3)room.transform.localPosition, rb);
				vertices.Add(vertex);
			} else {
				generatedRooms.Remove(room); // Remove null rooms (were deleted because of collision)
			}

		}

		delaunay = Delaunay3D.Triangulate(vertices);

		yield return null;
	}

	private IEnumerator CreateHallways() {
        List<Prim.Edge> edges = new List<Prim.Edge>();

		Debug.Log("Delaunay edges: " + delaunay.Edges.Count);

        foreach (var edge in delaunay.Edges) {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

		Debug.Log("Edge count: " + edges.Count);

        List<Prim.Edge> minimumSpanningTree = Prim.MinimumSpanningTree(edges, edges[0].U);

        selectedEdges = new HashSet<Prim.Edge>(minimumSpanningTree);
        var remainingEdges = new HashSet<Prim.Edge>(edges);
        remainingEdges.ExceptWith(selectedEdges);

        foreach (var edge in remainingEdges) {
            if (random.NextDouble() < 0.125) {
                selectedEdges.Add(edge);
            }
        }

		yield return null;
    }

	private IEnumerator PathfindHallways() {
        DungeonPathfinder3D aStar = new DungeonPathfinder3D(new Vector3Int(mapWidth, mapHeight, mapLength));

        foreach (var edge in selectedEdges) {
            var startRoom = (edge.U as Vertex<RoomBuilder>).Item;
            var endRoom = (edge.V as Vertex<RoomBuilder>).Item;

			Bounds startRoomBounds = startRoom.GetComponent<BoxCollider>().bounds;
			Bounds endRoomBounds = endRoom.GetComponent<BoxCollider>().bounds;

            var startPosf = startRoomBounds.center;
            var endPosf = endRoomBounds.center;
            var startPos = new Vector3Int((int)startPosf.x, (int)startPosf.y, (int)startPosf.z);
            var endPos = new Vector3Int((int)endPosf.x, (int)endPosf.y, (int)endPosf.z);

            var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder3D.Node a, DungeonPathfinder3D.Node b) => {
                var pathCost = new DungeonPathfinder3D.PathCost();

                var delta = b.Position - a.Position;

                if (delta.y == 0) {
                    //flat hallway
                    pathCost.cost = Vector3Int.Distance(b.Position, endPos);    //heuristic

                    if (grid[b.Position] == CellType.Stairs) {
                        return pathCost;
                    } else if (grid[b.Position] == CellType.Room) {
                        pathCost.cost += 5;
                    } else if (grid[b.Position] == CellType.None) {
                        pathCost.cost += 1;
                    }

                    pathCost.traversable = true;
                } else {
                    //staircase
                    if ((grid[a.Position] != CellType.None && grid[a.Position] != CellType.Hallway)
                        || (grid[b.Position] != CellType.None && grid[b.Position] != CellType.Hallway)) return pathCost;

                    pathCost.cost = 100 + Vector3Int.Distance(b.Position, endPos);    //base cost + heuristic

                    int xDir = Mathf.Clamp(delta.x, -1, 1);
                    int zDir = Mathf.Clamp(delta.z, -1, 1);
                    Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                    Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                    if (!grid.InBounds(a.Position + verticalOffset)
                        || !grid.InBounds(a.Position + horizontalOffset)
                        || !grid.InBounds(a.Position + verticalOffset + horizontalOffset)) {
                        return pathCost;
                    }

                    if (grid[a.Position + horizontalOffset] != CellType.None
                        || grid[a.Position + horizontalOffset * 2] != CellType.None
                        || grid[a.Position + verticalOffset + horizontalOffset] != CellType.None
                        || grid[a.Position + verticalOffset + horizontalOffset * 2] != CellType.None) {
                        return pathCost;
                    }

                    pathCost.traversable = true;
                    pathCost.isStairs = true;
                }

                return pathCost;
            });

            if (path != null) {
                for (int i = 0; i < path.Count; i++) {
                    var current = path[i];

                    if (grid[current] == CellType.None) {
                        grid[current] = CellType.Hallway;
                    }

                    if (i > 0) {
                        var prev = path[i - 1];

                        var delta = current - prev;

                        if (delta.y != 0) {
                            int xDir = Mathf.Clamp(delta.x, -1, 1);
                            int zDir = Mathf.Clamp(delta.z, -1, 1);
                            Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                            Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);
                            
                            grid[prev + horizontalOffset] = CellType.Stairs;
                            grid[prev + horizontalOffset * 2] = CellType.Stairs;
                            grid[prev + verticalOffset + horizontalOffset] = CellType.Stairs;
                            grid[prev + verticalOffset + horizontalOffset * 2] = CellType.Stairs;

                            PlaceStairs(prev + horizontalOffset);
                            PlaceStairs(prev + horizontalOffset * 2);
                            PlaceStairs(prev + verticalOffset + horizontalOffset);
                            PlaceStairs(prev + verticalOffset + horizontalOffset * 2);
                        }

                        Debug.DrawLine(prev + new Vector3(0.5f, 0.5f, 0.5f), current + new Vector3(0.5f, 0.5f, 0.5f), Color.blue, 100, false);
                    }
                }

                foreach (var pos in path) {
                    if (grid[pos] == CellType.Hallway) {
                        PlaceHallway(pos);
                    }
                }
            }
        }

		yield return null;
    }

	void PlaceCube(Vector3Int location, Vector3Int size) {
        GameObject go = Instantiate(cubePrefab, new Vector3(location.x, location.y, location.z), Quaternion.identity);
        go.GetComponent<Transform>().localScale = new Vector3(size.x, 1, size.y);
    }

	void PlaceHallway(Vector3Int location) {
		Debug.Log("LOCATION: " + location.ToString());
        PlaceCube(location, new Vector3Int(1, 1, 1));
    }

	void PlaceRoom(Vector3Int location, Vector3Int size) {
        PlaceCube(location, size);
    }

	void PlaceStairs(Vector3Int location) {
        PlaceCube(location, new Vector3Int(1, 1, 1));
    }
}
