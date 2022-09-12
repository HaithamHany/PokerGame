using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to handle all custom game events
/// </summary>
public class EventsHandler : MonoBehaviour
{
    private static EventsHandler _instance;
    public static EventsHandler Instance => _instance;
    
    //Events
    public event Action<int> OnPlayerPlacedBet;
    public event Action<int> OnBetAmountChanged;
    public event Action OnRemotePlayerPlacedBet;
    public event Action OnTurnStarted;


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
        }
    }
    
    public void PlayerPlacedBet(int amount)
    {
        if (OnPlayerPlacedBet != null)
        {
            OnPlayerPlacedBet(amount);
        }
    }
    
    public void BetAmountChanged(int amount)
    {
        if (OnBetAmountChanged != null)
        {
            OnBetAmountChanged(amount);
        }
    }

    public void TurnStarted()
    {
        if (OnTurnStarted != null)
        {
            OnTurnStarted();
        }
    }
    
    public void RemotePlayerPlacedBet()
    {
        if (OnRemotePlayerPlacedBet != null)
        {
            OnRemotePlayerPlacedBet();
        }
    }
}