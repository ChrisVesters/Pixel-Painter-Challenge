using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
	public static LevelManager Instance { get; private set; }

	public Color currentColor { get; set; } = Color.white;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}

		Instance = this;
	}
}