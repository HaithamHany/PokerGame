using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsHandler : MonoBehaviour
{
    public static EventsHandler Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }

    public event Action<int> OnActivatingObject;
    public event Action<int> OnDeactivatingObject;

    public void ActivatingObject(int objId)
    {
        if(OnActivatingObject != null)
        {
            OnActivatingObject(objId);
        }
    }
    public void DeactivatingObject(int objId)
    {
        if (OnDeactivatingObject != null)
        {
            OnDeactivatingObject(objId);
        }
    }
}