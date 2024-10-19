using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// using System.Numerics;
// using Unity.VisualScripting;
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

	private Color currentColor = Color.yellow;

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
				ClearAllHighlight();
				points.Clear();
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
					FillShape();
					ClearAllHighlight();
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

	// Does not include the start point, but does include the end point.
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
		Color color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.5f);
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

	private void ClearAllHighlight()
	{
		if (points.Count == 0)
		{
			return;
		}

		ClearHighlight(points[0]);
		for (int index = 0; index < points.Count; index++)
		{
			Vector3Int start = points[index];
			Vector3Int end = points[(index + 1) % points.Count];
			ClearHighlight(start, end);
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

	private void FillShape()
	{
		// Fill(points[0]);
		// for (int index = 0; index < points.Count; index++)
		// {
		// 	Vector3Int start = points[index];
		// 	Vector3Int end = points[(index + 1) % points.Count];
		// 	Fill(start, end);
		// }

		// Determine min and max x and y.
		// For all points inside of the boundary:
		// See if they are in the shape.
		// How do we know if they are in the shape?
		// Cast a 'ray' from the point and count how many segments we cross.

		// TODO: We have to use the lines, because of perpendicular lines.
		// Points that are an extension of vertical lines can be falsely marked as inside the shape.
		// TODO: we should remove these lines from checking.
		// TODO: optimization: we don't need to look at all points every time.
		// TODO: if the previous point (above us) was inside the shape,
		// just see if the current point is a border.\

		List<int>[,] lineGrid = new List<int>[(2 * GRID_SIZE_X), (2 * GRID_SIZE_Y)];
		for (int index = 0; index < points.Count; index++)
		{
			Vector3Int start = points[index];
			Vector3Int end = points[(index + 1) % points.Count];

			List<Vector3Int> positions = new() {start};
			foreach (Vector3Int position in GetLinePositions(start, end))
			{
				positions.Add(position);
			}

			foreach (Vector3Int position in positions)
			{
				List<int> currentLines = lineGrid[position.x + GRID_SIZE_X, position.y + GRID_SIZE_Y];
				if (currentLines == null)
				{
					currentLines = new List<int>();
					lineGrid[position.x + GRID_SIZE_X, position.y + GRID_SIZE_Y] = currentLines;
				}

				Debug.Log("Adding " + index + " to " + position);
				currentLines.Add(index);
			}
		}

		for (int y = -GRID_SIZE_Y; y < GRID_SIZE_Y; ++y)
		{
			bool shapeMode = false;
			ISet<int> crossedLines = new HashSet<int>();
			for (int x = -GRID_SIZE_X; x < GRID_SIZE_X; ++x)
			{
				List<int> lines = lineGrid[x + GRID_SIZE_X, y + GRID_SIZE_Y];
				// Debug.Log(x + ", " + y + ": " + lines != null ? lines.Count : 0);
				// bool highlighted = tilemap.HasTile(pos);

				// if (!shapeMode && highlighted) {
				// 	shapeMode = true;
				// } else if (shapeMode && highlighted) {
				// 	shapeMode = false;
				// }

				if (lines != null)
				{
					foreach (int line in lines)
					{
						crossedLines.Add(line);
					}

					// shapeMode = !shapeMode;
				}

				// if (highlighted)
				// {
				// 	shapeMode = !shapeMode;
				// }

				if (lines != null || crossedLines.Count % 2 != 0)
				{
					Vector3Int pos = new Vector3Int(x, y, Z_LAYER_HIGHLIGHT);
					Fill((Vector2Int)pos);
				}

				// if (highlighted || shapeMode)
				// {
				// 	Fill((Vector2Int)pos);
				// }
			}
		}

		// bool[,] fillGrid = new bool[GRID_SIZE_X * 2, GRID_SIZE_Y * 2];
		// for (int x = -GRID_SIZE_X; x < GRID_SIZE_X; ++x)
		// {
		// 	for (int y = -GRID_SIZE_Y; y < GRID_SIZE_Y; ++y)
		// 	{
		// 		Vector3Int pos = new Vector3Int(x, y, Z_LAYER_HIGHLIGHT);
		// 		fillGrid[x + GRID_SIZE_X, y + GRID_SIZE_Y] = tilemap.HasTile(pos);
		// 	}
		// }

		// for (int x = -GRID_SIZE_X; x < GRID_SIZE_X; ++x)
		// {
		// 	for (int y = -GRID_SIZE_Y; y < GRID_SIZE_Y; ++y)
		// 	{
		// 		bool highlighted = fillGrid[x + GRID_SIZE_X, y + GRID_SIZE_Y];
		// 		if (highlighted)
		// 		{
		// 			Fill(new Vector2Int(x, y));
		// 			continue;
		// 		}

		// 		int edges = 0;
		// 		for (int index = -GRID_SIZE_Y; index < y; index++)
		// 		{
		// 			edges += fillGrid[x + GRID_SIZE_X, index + GRID_SIZE_Y] ? 1 : 0;
		// 		}

		// 		if (edges % 2 != 0)
		// 		{
		// 			Fill(new Vector2Int(x, y));
		// 		}
		// 	}
		// }
	}

	private void Fill(Vector2Int cell)
	{
		Color color = new Color(currentColor.r, currentColor.g, currentColor.b, 1f);
		Vector3Int fillPos = new Vector3Int(cell.x, cell.y, Z_LAYER_FILL);
		tilemap.SetColor(fillPos, color);
	}

	private void Fill(Vector3Int start, Vector3Int end)
	{
		Vector3Int[] positions = GetLinePositions(start, end);
		for (int i = 0; i < positions.Length; ++i)
		{
			// Fill(positions[i]);
		}
	}
}
