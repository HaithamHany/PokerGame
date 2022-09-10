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
    public event Action OnAllBetsPlaced;
    public event Action<EPlayer> OnPlayerTurn;
    public event Action<ERoundResult> OnRoundDone;
    public event Action OnBettingObjectStarted;
    public event Action OnBettingObjectEnded;
    public event Action OnStackCreated;
    public event Action OnBetCanceled;


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
    
    public void AllBetsPlaced()
    {
        if (OnAllBetsPlaced != null)
        {
            OnAllBetsPlaced();
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
}

public enum EPlayer
{
    Player01 = 0,
    Player02 = 1
}

public enum ERoundResult
{
    Win = 0,
    Lose = 1,
}