using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Vector2 mapSize;

    [Range(0, 1)]
    public float outlinePercent;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = this.transform;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                float tileX = -mapSize.x/2 + .5f + x;
                float tileZ = -mapSize.y/2 + .5f + y;
                Vector3 tilePosition = new Vector3(tileX, 0, tileZ);
                Transform newTile = (Transform) Instantiate(tilePrefab, tilePosition, tilePrefab.rotation);
                newTile.localScale = Vector3.one * (1 - outlinePercent);
                newTile.parent = mapHolder;
            }
        }
    }

}
