using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsHandler : MonoBehaviour
{
    private static EventsHandler _instance;
    public static EventsHandler Instance => _instance;
    
    //Events
    public event Action<int> OnActivatingObject;
    public event Action<int> OnDeactivatingObject;
    public event Action<EPlayer, float> OnPlayerPlacedBet;
    public event Action OnAllBetsPlaced;
    public event Action<EPlayer> OnPlayerTurn;
    public event Action<ERoundResult> OnRoundDone;


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
    
    public void DeactivatingObject(EPlayer player, float amount)
    {
        if (OnPlayerPlacedBet != null)
        {
            OnPlayerPlacedBet(player, amount);
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
    
    public void RoundResult(ERoundResult result)
    {
        if (OnRoundDone != null)
        {
            OnRoundDone(result);
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