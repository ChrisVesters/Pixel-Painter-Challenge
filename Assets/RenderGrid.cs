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

	private const int Z_LAYER_BORDER = -1;
	private const int Z_LAYER_FILL = 0;
	private const int Z_LAYER_HIGHLIGHT = 1;

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
				Vector3Int borderPos = new Vector3Int(x, y, Z_LAYER_BORDER);
				Vector3Int pos = new Vector3Int(x, y, Z_LAYER_FILL);

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

		// Change the painting mechanism.
		// Click on pixels to create a shape.
		// Note: Only horizontal, vertical and diagonal lines are allowed.
		// Close the shape (and paint it) by clicking on the start pixel.
		// Note: Single pixel paining is not possible and simply clears it.
		// Note: You can only remove the last point (by clicking on it).
		// Note only convex shapes are allowed
		if (Input.GetMouseButtonDown(0))
		{
			if (IsValidTilePosition(cellPos))
			{
				coloringStartPoint = cellPos;

				highlight(cellPos);
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

			if (coloringStartPoint.HasValue)
			{
				clearHightlight(coloringStartPoint.Value);
			}

			coloringStartPoint = null;
		}

		if (Input.GetMouseButton(0))
		{
			// TODO: if cursor out of bounds, clear highlight?
			if (IsValidTilePosition(cellPos))
			{
				// Highlight up to here from the start.
				highlight(cellPos);
			}
		}
	}

	private bool IsValidTilePosition(Vector3Int position)
	{
		return tilemap.GetTile(position) != null;
	}

	private bool isStraightLine(Vector3Int start, Vector3Int end)
	{
		return start.x == end.x || start.y == end.y;
	}

	private void highlight(Vector3Int cell)
	{
		Tile fill = Resources.Load<Tile>("Fill");

		Color source = Color.yellow;
		Color color = new Color(source.r, source.g, source.b, 0.5f);
		Vector3Int highlightPos = new Vector3Int(cell.x, cell.y, Z_LAYER_HIGHLIGHT);

		tilemap.SetTile(highlightPos, fill);
		tilemap.SetTileFlags(highlightPos, TileFlags.None);
		tilemap.SetColor(highlightPos, color);
	}

	private void clearHightlight(Vector3Int cell)
	{
		Vector3Int highlightPos = new Vector3Int(cell.x, cell.y, Z_LAYER_HIGHLIGHT);
		tilemap.SetTile(highlightPos, null);
	}
}
