using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

public class RenderGrid : MonoBehaviour
{
	const int GRID_X = 10;
	const int GRID_Y = 10;
	// public GameObject prefab;

	public Tilemap gridTileMap;

	// Start is called before the first frame update
	void Start()
	{
		var tile = Resources.Load("Tile");
		var square = Resources.Load("Tiles");
		//Debug.Log("Tile name:" + tile.name);
		//Debug.Log("Tile sprite:" + tile.sprite);

		Debug.Log(square);
		Debug.Log(tile);

		// TODO: get the actual tilemap.
		// TODO: Get the individual tiles.
		// TODO: use those to actually render stuff.

		// GameObject tilemap = GameObject.Find("Tilemap");
		for (int y = 0; y < GRID_Y; ++y)
		{
			for (int x = 0; x < GRID_X; ++x)
			{
				// gridTileMap.SetTile(new Vector3Int(x, y, 0), tile);
				// Instantiate(prefab, new Vector3(x - GRID_X / 2, y - GRID_Y / 2, 0), Quaternion.identity, tilemap.transform);
			}
		}

		gridTileMap.CompressBounds();
	}

	// Update is called once per frame
	void Update()
	{

	}
}
