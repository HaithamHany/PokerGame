using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stack : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;

    public void Init()
    {
        _renderer.material.color = GenerateRandomColor();
    }

    private Color GenerateRandomColor()
    {
        return Random.ColorHSV();
    }
}
