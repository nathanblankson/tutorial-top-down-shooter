using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Vector2Int mapSize;
    public int mapSeed = 10;

    [Range(0f, 1f)]
    public float obstaclePercent;
    [Range(0f, 1f)]
    public float outlinePercent;

    private List<Coord> _allTileCoords;
    private Queue<Coord> _shuffledTileCoords;

    private Coord _mapCenter;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        _allTileCoords = new List<Coord>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                _allTileCoords.Add(new Coord(x, y));
            }
        }
        _shuffledTileCoords = new Queue<Coord> (Utility.ShuffleArray(_allTileCoords.ToArray(), mapSeed));
        _mapCenter = new Coord((int) mapSize.x/2, (int) mapSize.y/2);

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

        bool[,] obstacleMap = new bool[(int) mapSize.x, (int) mapSize.y];

        int targetObstacleCount = (int) (mapSize.x * mapSize.y * obstaclePercent);
        int currentObstacleCount = 0;

        for (int i = 0; i < targetObstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != _mapCenter && IsMapFullyAccessible(obstacleMap, currentObstacleCount))
            {
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                Vector3 offset = Vector3.up * .5f;
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + offset, obstaclePrefab.rotation);
                newObstacle.parent = obstacleHolder;
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }
    }

    private bool IsMapFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        // Using flood fill
        int xLength = obstacleMap.GetLength(0);
        int yLength = obstacleMap.GetLength(1);

        bool[,] mapFlags = new bool[xLength, yLength];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(_mapCenter);
        mapFlags[_mapCenter.x, _mapCenter.y] = true;

        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            // Loop through adjacent tiles
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;

                    // Ignore diagonal tiles
                    if (x == 0 ^ y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < xLength && neighbourY >= 0 && neighbourY < yLength)
                        {
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                // Found unchecked adjacent neighbour that is not obstacle
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int) (mapSize.x * mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    private Vector3 CoordToPosition(int x, int y)
    {
        float tileX = -mapSize.x/2 + .5f + x;
        float tileZ = -mapSize.y/2 + .5f + y;
        return new Vector3(tileX, 0, tileZ);
    }

    private Coord GetRandomCoord()
    {
        Coord randomCoord =  _shuffledTileCoords.Dequeue();
        _shuffledTileCoords.Enqueue(randomCoord);
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

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }

}
