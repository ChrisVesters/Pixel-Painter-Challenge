using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{

    private Button button;

    public void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
    }

    void HandleClick()
    {
        LevelManager.Instance.currentColor = button.image.color;
    }
}
