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
		Tile fill = Resources.Load<Tile>("Cells/Black");
		Tile fillEvent = Resources.Load<Tile>("Cells/White");

		for (int y = -GRID_SIZE; y < GRID_SIZE; ++y)
		{
			for (int x = -GRID_SIZE; x < GRID_SIZE; ++x)
			{
				Vector3Int pos = new Vector3Int(x, y, 0);

				tilemap.SetTile(new Vector3Int(x, y, 10), border);
				if (Math.Abs(x % 2) == Math.Abs(y % 2))
				{
					tilemap.SetTile(pos, fillEvent);
				}
				else
				{
					tilemap.SetTile(new Vector3Int(x, y, 0), fill);
				}

				Color c = new Color(255f, 255f, 0f, 1f);
				tilemap.SetColor(pos, c);
				tilemap.RefreshTile(pos);
			}
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
