using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navMeshFloor;
    public Transform navMeshMaskPrefab;
    public Vector2Int maxMapSize;

    [Range(0f, 1f)]
    public float outlinePercent;
    public float tileSize = 1f;

    private List<Coord> _allTileCoords;
    private Queue<Coord> _shuffledTileCoords;

    private Map _currentMap;
    private Color _randomForegroundColor;
    private Color _randomBackgroundColor;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        _currentMap = maps[mapIndex];
        System.Random prng = new System.Random(_currentMap.seed);

        // Set box collider (floor)
        GetComponent<BoxCollider>().size = new Vector3(_currentMap.mapSize.x * tileSize, .5f, _currentMap.mapSize.y * tileSize);

        // Generating coords
        _allTileCoords = new List<Coord>();
        for (int x = 0; x < _currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < _currentMap.mapSize.y; y++)
            {
                _allTileCoords.Add(new Coord(x, y));
            }
        }
        _shuffledTileCoords = new Queue<Coord> (Utility.ShuffleArray(_allTileCoords.ToArray(), _currentMap.seed));

        // Instantiate map holder
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        // Instantiate tiles
        Transform tileHolder = new GameObject("Tiles").transform;
        tileHolder.parent = mapHolder;

        for (int x = 0; x < _currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < _currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = (Transform) Instantiate(tilePrefab, tilePosition, tilePrefab.rotation);
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = tileHolder;
            }
        }

        // Instantiate obstacles
        Transform obstacleHolder = new GameObject("Obstacles").transform;
        obstacleHolder.parent = mapHolder;

        bool[,] obstacleMap = new bool[(int) _currentMap.mapSize.x, (int) _currentMap.mapSize.y];

        int targetObstacleCount = (int) (_currentMap.mapSize.x * _currentMap.mapSize.y * _currentMap.obstaclePercent);
        int currentObstacleCount = 0;

        // Random colour
        if (_currentMap.isRandomColours)
        {
            _randomForegroundColor = Random.ColorHSV(0, 1f);
            _randomBackgroundColor = Random.ColorHSV(0, 1f);
        }

        for (int i = 0; i < targetObstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != _currentMap.mapCenter && IsMapFullyAccessible(obstacleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(_currentMap.minObstacleHeight, _currentMap.maxObstacleHeight, (float) prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up *  obstacleHeight / 2, obstaclePrefab.rotation);
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
                newObstacle.parent = obstacleHolder;

                // Set colour
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);

                float colourPercent;
                if (_currentMap.isGradientPattern)
                {
                    colourPercent = randomCoord.y / (float) _currentMap.mapSize.y;
                }
                else
                {
                    colourPercent = (float) prng.NextDouble();
                }

                if (_currentMap.isRandomColours)
                {
                    obstacleMaterial.color = Color.Lerp(_randomForegroundColor, _randomBackgroundColor, colourPercent);
                }
                else
                {
                    obstacleMaterial.color = Color.Lerp(_currentMap.foregroundColour, _currentMap.backgroundColour, colourPercent);
                }

                obstacleRenderer.sharedMaterial = obstacleMaterial;
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        // Instantiate borders
        Transform maskHolder = new GameObject("Masks").transform;
        maskHolder.parent = mapHolder;

        Transform maskLeft = (Transform) Instantiate(navMeshMaskPrefab, Vector3.left * (_currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity);
        maskLeft.gameObject.name = "Left";
        maskLeft.parent = maskHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - _currentMap.mapSize.x) / 2f, 1f, _currentMap.mapSize.y) * tileSize;

        Transform maskRight = (Transform) Instantiate(navMeshMaskPrefab, Vector3.right * (_currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity);
        maskRight.gameObject.name = "Right";
        maskRight.parent = maskHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - _currentMap.mapSize.x) / 2f, 1f, _currentMap.mapSize.y) * tileSize;

        Transform maskTop = (Transform) Instantiate(navMeshMaskPrefab, Vector3.forward * (_currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        maskTop.gameObject.name = "Top";
        maskTop.parent = maskHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1f, (maxMapSize.y - _currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskBottom = (Transform) Instantiate(navMeshMaskPrefab, Vector3.back * (_currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity);
        maskBottom.gameObject.name = "Bottom";
        maskBottom.parent = maskHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1f, (maxMapSize.y - _currentMap.mapSize.y) / 2f) * tileSize;

        // Walkable floor
        navMeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y, 1f) * tileSize;
    }

    private bool IsMapFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        // Using flood fill
        int xLength = obstacleMap.GetLength(0);
        int yLength = obstacleMap.GetLength(1);

        bool[,] mapFlags = new bool[xLength, yLength];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(_currentMap.mapCenter);
        mapFlags[_currentMap.mapCenter.x, _currentMap.mapCenter.y] = true;

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

        int targetAccessibleTileCount = (int) (_currentMap.mapSize.x * _currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    private Vector3 CoordToPosition(int x, int y)
    {
        float tileX = -_currentMap.mapSize.x / 2f + .5f + x;
        float tileZ = -_currentMap.mapSize.y / 2f + .5f + y;
        return new Vector3(tileX, 0, tileZ) * tileSize;
    }

    private Coord GetRandomCoord()
    {
        Coord randomCoord =  _shuffledTileCoords.Dequeue();
        _shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

}
