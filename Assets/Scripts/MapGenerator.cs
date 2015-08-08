using UnityEngine;
using System.Collections;
using System;

public class MapGenerator : MonoBehaviour {

	public int width;
	public int height;

	public string seed;
	public bool useRandomSeed;

	[Range(0,100)]
	public int randomFilePercent;	// Guideline of how much should be filled
	int[,] map;						// Defines a grid of intergers

	void Start()
	{
		GenerateMap();
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			GenerateMap();
		}
	}

	void GenerateMap()
	{
		map = new int[width, height];
		RandomFillMap();
		for (int i = 0; i < 5; i++)
		{
			SmoothMap();
		}

        int borderSize = 1;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                // If the x and y value are inside the border
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x,y] = 1;
                }
            }
        }

		MeshGenerator meshGen = GetComponent<MeshGenerator>();
		meshGen.GenerateMesh(borderedMap, 1);
	}

	void RandomFillMap()
	{
		if (useRandomSeed)
		{
			seed = Time.time.ToString();
		}

		System.Random psuedoRandom = new System.Random(seed.GetHashCode());

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
				{
					map[x,y] = 1;
				}
				else {
					map[x,y] = (psuedoRandom.Next(0,100) < randomFilePercent) ? 1:0;
				}
			}
		}
	}

	void SmoothMap()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				int neighbourWallTiles = GetSurroundingWallCount(x,y);
				if (neighbourWallTiles > 4)
				{
					map[x, y] = 1;
				}
				else if (neighbourWallTiles < 4)
				{
					map[x, y] = 0;
				}
			}
		}
	}

	int GetSurroundingWallCount(int gridX, int gridY)
	{
		int wallCount = 0;
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
		{
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
			{
				if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
				{
					if (neighbourX != gridX || neighbourY != gridY)
					{
						wallCount += map[neighbourX, neighbourY];
					}
				}
				else {
					wallCount++;
				}
			}
		}
		return wallCount;
	}
}
