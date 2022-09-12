using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsHandler : MonoBehaviour
{
    private static EventsHandler _instance;
    public static EventsHandler Instance => _instance;
    
    //Events
    public event Action<int> OnPlayerPlacedBet;
    public event Action<int> OnPlayerRemovedBet;
    public event Action<int> OnBetAmountChanged;
    public event Action OnRemotePlayerPlacedBet;
    public event Action OnTurnStarted;
    public event Action OnAllBetsPlaced;
    public event Action<EPlayer> OnPlayerTurn;
    public event Action<ERoundResult> OnRoundDone;
    public event Action OnBettingObjectStarted;
    public event Action OnBettingObjectEnded;
    public event Action OnStackCreated;
    public event Action OnBetCanceled;
    public event Action OnChangeColorRequested;


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
    
    public void PlayerRemovedBet(int amount)
    {
        if (OnPlayerRemovedBet != null)
        {
            OnPlayerRemovedBet(amount);
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
    
    public void PlayerTurn(EPlayer player)
    {
        if (OnPlayerTurn != null)
        {
            OnPlayerTurn(player);
        }
    }
    
    public void BettingObjectStarted()
    {
        if (OnBettingObjectStarted != null)
        {
            OnBettingObjectStarted();
        }
    }
    
    public void BettingObjectEnded()
    {
        if (OnBettingObjectEnded != null)
        {
            OnBettingObjectEnded();
        }
    }
    
    public void StackCreated()
    {
        if (OnStackCreated != null)
        {
            OnStackCreated();
        }
    }
    
    public void BetCanceled()
    {
        if (OnBetCanceled != null)
        {
            OnBetCanceled();
        }
    }
    
    public void ChangeColorRequest()
    {
        if (OnChangeColorRequested != null)
        {
            OnChangeColorRequested();
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

public enum EPlayer
{
    None = 0,
    Player01 = 1,
    Player02 = 2
}

public enum ERoundResult
{
    Win = 0,
    Lose = 1,
}