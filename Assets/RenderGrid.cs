using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RenderGrid : MonoBehaviour
{
	private const int GRID_X = 10;
	private const int GRID_Y = 10;

	void Start()
	{
		Tilemap tilemap = GetComponent<Tilemap>();
		Tile whiteTile = Resources.Load<Tile>("Tiles/White");

		for (int y = 0; y < GRID_Y; ++y)
		{
			for (int x = 0; x < GRID_X; ++x)
			{
				tilemap.SetTile(new Vector3Int(x, y, 0), whiteTile);
			}
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
