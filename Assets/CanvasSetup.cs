using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class CanvasSetup : MonoBehaviour
{
    private const int START_X = 122;

    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        Button paintPrefab = Resources.Load<Button>("Paint");

        // TODO: get rid of magic numbers.
        int gridWidth = (LevelManager.Instance.gridSizeX * 2) * 26;
        int width = gridWidth - 50;

        Color[] colors = LevelManager.Instance.getPaintColors();
        for (int index = 0; index < colors.Length; index++)
        {
            int x = START_X + (width / (colors.Length - 1)) * index;
            Button paintButton = Instantiate(paintPrefab, new Vector3(x, 60, 0), Quaternion.identity, canvas.transform);
            paintButton.image.color = colors[index];
        }
    }
}
