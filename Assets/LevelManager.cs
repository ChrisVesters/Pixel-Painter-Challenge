using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
	public static LevelManager Instance { get; private set; }

	public int gridSizeX { get; private set; } = 6;
	public int gridSizeY { get; private set; } = 6;

	private Color[,] levelColors;
	public Color currentColor { get; set; } = Color.white;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}
		Instance = this;

		levelColors = new Color[2 * gridSizeY, 2 * gridSizeX];
		Color[] colours = new Color[] { Color.yellow, Color.red, Color.green, Color.blue, Color.cyan, Color.magenta };
		for (int y = -gridSizeY; y < gridSizeY; ++y)
		{
			int yIndex = y + gridSizeY;
			for (int x = -gridSizeX; x < gridSizeX; ++x)
			{
				int xIndex = x + gridSizeX;
				levelColors[yIndex, xIndex] = (colours[(Math.Abs(x) + Math.Abs(y)) % 6]);
			}
		}
	}

	public Color getCellColor(Vector2Int cell)
	{
		int y = cell.y + gridSizeY;
		int x = cell.x + gridSizeX;
		return levelColors[y, x];
	}
}