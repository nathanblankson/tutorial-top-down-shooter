using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Vector2 mapSize;
    public int mapSeed = 10;
    public int obstacleCount;

    [Range(0, 1)]
    public float outlinePercent;

    private List<Coord> allTileCoords;
    private Queue<Coord> shuffledTileCoords;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        allTileCoords = new List<Coord>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coord> (Utility.ShuffleArray(allTileCoords.ToArray(), mapSeed));

        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        Transform tileHolder = new GameObject("Tiles").transform;
        tileHolder.parent = mapHolder;

        Transform obstacleHolder = new GameObject("Obstacles").transform;
        obstacleHolder.parent = mapHolder;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = (Transform) Instantiate(tilePrefab, tilePosition, tilePrefab.rotation);
                newTile.localScale = Vector3.one * (1 - outlinePercent);
                newTile.parent = tileHolder;
            }
        }

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
            Vector3 offset = Vector3.up * .5f;
            Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + offset, obstaclePrefab.rotation);
            newObstacle.parent = obstacleHolder;
        }
    }

    private Vector3 CoordToPosition(int x, int y)
    {
        float tileX = -mapSize.x/2 + .5f + x;
        float tileZ = -mapSize.y/2 + .5f + y;
        return new Vector3(tileX, 0, tileZ);
    }

    private Coord GetRandomCoord()
    {
        Coord randomCoord =  shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

}
