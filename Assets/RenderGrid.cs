using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// using System.Numerics;
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

	private readonly List<Vector3Int> points = new();
	private Vector3Int? hoverPosition = null;

	private Tile fill;

	void Start()
	{
		tilemap = GetComponent<Tilemap>();
		fill = Resources.Load<Tile>("Fill");

		Tile border = Resources.Load<Tile>("Border");

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
		// Note No crossing lines are allowed.
		if (Input.GetMouseButtonUp(0))
		{
			hoverPosition = null;

			if (!IsValidTilePosition(cellPos))
			{
				points.Clear();
				// TODO: remove all highlighting.
				return;
			}

			if (points.Count == 0)
			{
				Highlight(cellPos);
				points.Add(cellPos);
				return;
			}

			if (IsValidPoint(cellPos))
			{
				if (points.First() == cellPos)
				{
					Debug.Log("Completed shape");
					// TODO: color the shape.
					// TODO: remove highlight.
					points.Clear();
				}
				else
				{
					points.Add(cellPos);
				}
			}
		}

		if (points.Count == 0)
		{
			return;
		}

		if (hoverPosition.HasValue && IsValidPoint(hoverPosition.Value))
		{
			ClearHighlight(points.Last(), hoverPosition.Value);
		}

		if (IsValidPoint(cellPos))
		{
			Highlight(points.Last(), cellPos);
		}

		hoverPosition = cellPos;
	}

	private bool IsValidTilePosition(Vector3Int position)
	{
		return tilemap.GetTile(position) != null;
	}

	private bool IsValidPoint(Vector3Int end)
	{
		if (!IsValidTilePosition(end))
		{
			return false;
		}

		Vector3Int start = points.Last();
		return IsValidLine(start, end) && !CrossesOtherLines(start, end);
	}

	private bool IsValidLine(Vector3Int start, Vector3Int end)
	{
		int deltaX = start.x - end.x;
		int deltaY = start.y - end.y;

		if (deltaX == 0 && deltaY == 0)
		{
			return false;
		}

		return deltaX == 0 || deltaY == 0
			|| Math.Abs(deltaX) == Math.Abs(deltaY);
	}

	// To find orientation of ordered triplet (s(tart), m(iddle), e(nd)).
	// The function returns following values
	// 0 --> p, q and r are collinear
	// 1 --> Clockwise
	// -1 --> Counterclockwise
	private int Orientation(Vector3Int s, Vector3Int m, Vector3Int e)
	{
		return Math.Sign((m.y - s.y) * (e.x - m.x) - (m.x - s.x) * (e.y - m.y));
	}

	// Check if point lies on the segment se.
	// Note: This method only works if the points are collinear.
	private bool OnSegment(Vector3Int s, Vector3Int e, Vector3Int point)
	{
		bool overlapX = point.x <= Math.Max(s.x, e.x) && point.x >= Math.Min(s.x, e.x);
		bool overlapY = point.y <= Math.Max(s.y, e.y) && point.y >= Math.Min(s.y, e.y);

		return overlapX && overlapY;
	}

	private bool CrossesOtherLines(Vector3Int start, Vector3Int end)
	{
		// Well, no, not if there is no other segment yet.
		int startIndex = 0;
		if ((points.Count > 2) && (end == points.First()))
		{
			startIndex = 1;
		}

		for (int index = startIndex; index < points.Count - 1; index++)
		{
			Vector3Int lineStart = points[index];
			Vector3Int lineEnd = points[index + 1];

			int o1 = Orientation(start, end, lineStart);
			int o2 = Orientation(start, end, lineEnd);
			int o3 = Orientation(lineStart, lineEnd, start);
			int o4 = Orientation(lineStart, lineEnd, end);

			if (start == lineEnd)
			{
				// Check if segment goes back on previous segment.
				// If o1 is not 0, then the segements are not on the same line.
				if (o1 != 0)
				{
					return false;
				}

				// Check direction of segments.
				int xDirection = Math.Sign(end.x - start.x);
				int yDirection = Math.Sign(end.y - start.y);

				int lineXDirection = Math.Sign(lineEnd.x - lineStart.x);
				int lineYDirection = Math.Sign(lineEnd.y - lineStart.y);

				return (xDirection != lineXDirection) || (yDirection != lineYDirection);
			}

			if (o1 != o2 && o3 != o4)
			{
				return true;
			}

			Debug.Log(o1 + " " + o2 + " " + o3 + " " + o4);
			// Existing segment and new segment are on the same line.
			// Check for overlaps.
			if (o1 == 0 && OnSegment(start, end, lineStart))
			{
				return true;
			}

			if (o2 == 0 && OnSegment(start, end, lineEnd))
			{
				return true;
			}

			if (o3 == 0 && OnSegment(lineStart, lineEnd, start))
			{
				return true;
			}

			if (o4 == 0 && OnSegment(lineStart, lineEnd, end))
			{
				return true;
			}
		}

		return false;
	}

	private Vector3Int[] GetLinePositions(Vector3Int start, Vector3Int end)
	{
		int deltaX = end.x - start.x;
		int deltaY = end.y - start.y;

		if (deltaX == 0 && deltaY == 0)
		{
			return new Vector3Int[0] { };
		}

		int steps = Math.Max(Math.Abs(deltaX), Math.Abs(deltaY));
		int stepX = deltaX / steps;
		int stepY = deltaY / steps;

		Vector3Int[] positions = new Vector3Int[steps];
		for (int step = 1; step <= steps; ++step)
		{
			Vector3Int position = new(start.x + step * stepX, start.y + step * stepY, start.z);
			positions[step - 1] = position;
		}

		return positions;
	}

	private void Highlight(Vector3Int cell)
	{
		Color source = Color.yellow;
		Color color = new Color(source.r, source.g, source.b, 0.5f);
		Vector3Int highlightPos = new Vector3Int(cell.x, cell.y, Z_LAYER_HIGHLIGHT);

		tilemap.SetTile(highlightPos, fill);
		tilemap.SetTileFlags(highlightPos, TileFlags.None);
		tilemap.SetColor(highlightPos, color);
	}

	private void Highlight(Vector3Int start, Vector3Int end)
	{
		Vector3Int[] positions = GetLinePositions(start, end);
		for (int i = 0; i < positions.Length; ++i)
		{
			Highlight(positions[i]);
		}
	}

	private void ClearHighlight(Vector3Int cell)
	{
		Vector3Int highlightPos = new Vector3Int(cell.x, cell.y, Z_LAYER_HIGHLIGHT);
		tilemap.SetTile(highlightPos, null);
	}

	private void ClearHighlight(Vector3Int start, Vector3Int end)
	{
		Vector3Int[] positions = GetLinePositions(start, end);
		for (int i = 0; i < positions.Length; ++i)
		{
			// Don't clear the starting point.
			if (positions[i] == points[0])
			{
				continue;
			}

			ClearHighlight(positions[i]);
		}
	}
}
