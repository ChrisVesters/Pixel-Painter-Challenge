using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class LevelManager : MonoBehaviour
{
	public static LevelManager Instance { get; private set; }

	public TextAsset levelFile;

	public TextMeshProUGUI cellsLabel;
	public TextMeshProUGUI movesLabel;

	private LevelData levelData;

	public int gridSizeX { get; private set; } = 6;
	public int gridSizeY { get; private set; } = 6;

	private Color[,] levelColors;
	public Color currentColor { get; set; } = Color.white;

	private int correctCells = 0;
	private int movesLeft;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}
		Instance = this;

		levelData = JsonUtility.FromJson<LevelData>(levelFile.text);

		levelColors = new Color[2 * gridSizeY, 2 * gridSizeX];
		for (int y = -gridSizeY; y < gridSizeY; ++y)
		{
			int yIndex = y + gridSizeY;
			for (int x = -gridSizeX; x < gridSizeX; ++x)
			{
				int xIndex = x + gridSizeX;
				int offset = (yIndex * 2 * gridSizeX) + xIndex;
				levelColors[yIndex, xIndex] = levelData.colors[levelData.grid[offset]];
			}
		}

		movesLeft = levelData.moves;

		updateCellsLabel();
		updateMovesLabel();
	}

	public Color getCellColor(Vector2Int cell)
	{
		int y = cell.y + gridSizeY;
		int x = cell.x + gridSizeX;
		return levelColors[y, x];
	}

	public Color[] getPaintColors() {
		return levelData.colors;
	}

	public void increaseCorrectCells()
	{
		++correctCells;
		updateCellsLabel();
		// TODO: Check for win!
	}

	public void decreaseCorrectCells()
	{
		--correctCells;
		updateCellsLabel();
	}

	public void decreaseMovesLeft()
	{
		--movesLeft;
		updateMovesLabel();
		// TODO: Check for game over
	}

	private void updateCellsLabel()
	{
		cellsLabel.text = correctCells + "/" + (2 * gridSizeX) * (2 * gridSizeY);
	}

	private void updateMovesLabel()
	{
		movesLabel.text = movesLeft.ToString();
	}
}