using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Map
{
    public Coord mapSize = new Coord(10, 10);
    [Range(0f, 1f)]
    public float obstaclePercent = .2f;
    public int seed = 10;
    public float minObstacleHeight = 1f;
    public float maxObstacleHeight = 1f;
    public Color foregroundColour = Color.black;
    public Color backgroundColour = Color.black;
    public bool isRandomColours = false;
    public bool isGradientPattern = true;

    public Coord mapCenter {
        get
        {
            return new Coord(mapSize.x / 2, mapSize.y / 2);
        }
    }
}
