using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RenderGrid : MonoBehaviour
{
	private const int GRID_SIZE = 6;

	void Start()
	{
		Tilemap tilemap = GetComponent<Tilemap>();

		Tile border = Resources.Load<Tile>("Borders/Red");
		Tile fill = Resources.Load<Tile>("Cells/White");

		Color[] colours = new Color[] { Color.yellow, Color.red, Color.green, Color.blue, Color.cyan, Color.magenta };

		for (int y = -GRID_SIZE; y < GRID_SIZE; ++y)
		{
			for (int x = -GRID_SIZE; x < GRID_SIZE; ++x)
			{
				Vector3Int pos = new Vector3Int(x, y, 0);

				tilemap.SetTile(new Vector3Int(x, y, 10), border);
				tilemap.SetTile(pos, fill);
				tilemap.SetTileFlags(pos, TileFlags.None);
				tilemap.SetColor(pos, colours[(Math.Abs(x) + Math.Abs(y)) % 6]);
			}
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
