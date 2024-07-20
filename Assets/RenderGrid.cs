using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class RenderGrid : MonoBehaviour
{
	private const int GRID_SIZE_X = 6;
	private const int GRID_SIZE_Y = 6;

	private Tilemap tilemap;

	private Vector3Int? coloringStartPoint;

	void Start()
	{
		tilemap = GetComponent<Tilemap>();

		Tile border = Resources.Load<Tile>("Border");
		Tile fill = Resources.Load<Tile>("Fill");

		Color[] colours = new Color[] { Color.yellow, Color.red, Color.green, Color.blue, Color.cyan, Color.magenta };

		for (int y = -GRID_SIZE_Y; y < GRID_SIZE_Y; ++y)
		{
			for (int x = -GRID_SIZE_X; x < GRID_SIZE_X; ++x)
			{
				Vector3Int borderPos = new Vector3Int(x, y, 10);
				Vector3Int pos = new Vector3Int(x, y, 0);

				tilemap.SetTile(borderPos, border);
				tilemap.SetTileFlags(borderPos, TileFlags.None);
				tilemap.SetColor(borderPos, colours[(Math.Abs(x) + Math.Abs(y)) % 6]);

				tilemap.SetTile(pos, fill);
				tilemap.SetTileFlags(pos, TileFlags.None);
				// tilemap.SetColor(pos, colours[Math.Abs(x) * Math.Abs(y) % 6]);
			}
		}
	}

	void Update()
	{
		Vector3 mousePos = Input.mousePosition;
		Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
		Vector3Int cellPos = tilemap.WorldToCell(worldPos);

		if (Input.GetMouseButtonDown(0))
		{
			if (IsValidTilePosition(cellPos))
			{
				coloringStartPoint = cellPos;
			}
		}

		if (Input.GetMouseButtonUp(0))
		{
			if (IsValidTilePosition(cellPos))
			{
				if (coloringStartPoint.HasValue)
				{
					tilemap.SetColor(coloringStartPoint.Value, Color.black);
					tilemap.SetColor(cellPos, Color.black);
				}
			}
			coloringStartPoint = null;
		}
	}

	private bool IsValidTilePosition(Vector3Int position)
	{
		return tilemap.GetTile(position) != null;
	}
}
