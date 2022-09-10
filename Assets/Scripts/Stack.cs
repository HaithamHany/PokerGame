using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Stack : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;

    private void Start()
    {
        EventsHandler.Instance.OnChangeColorRequested += ChangeColorRequested;
    }
    
    public void Init()
    {
        _renderer.material.color = GenerateRandomColor();
    }

    private Color GenerateRandomColor()
    {
        return Random.ColorHSV();
    }
    
    private void ChangeColorRequested()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        _renderer.material.color += GenerateRandomColor();
    }

    private void OnDestroy()
    {
        EventsHandler.Instance.OnChangeColorRequested -= ChangeColorRequested;
    }
}
