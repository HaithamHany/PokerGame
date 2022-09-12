﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("Stacks Generator")]
    [Space]
    [SerializeField] private Stack _stack;
    [SerializeField] private int _coloumnLength;
    [SerializeField] private int _rowLength;
    [SerializeField] private float _xSpace;
    [SerializeField] private float _zSpace;

    [Header("Bet Handling")] [Space] 
    [SerializeField] private TextMeshProUGUI _betAmountTxt;
    [SerializeField] private Button _addBetButton;
    [SerializeField] private Button _removeBetButton;
    
    //betting stacks
    private List<Stack> _localBettingStacks = new List<Stack>();
    private List<Stack> _remoteBettingStacks = new List<Stack>();
    private int _localBetPlacedAmount;
    private int _currentLocalStack = 0;
    private int _currentRemoteStack = 0;
    
    private const int MAX_STACKS_LIMIT = 10;
    private const int BET_INCREMENT = 10;
    private const int LOCAL_X_OFFSET = 4;
    private const int LOCAL_Z_OFFSET = 3;
    private const int REMOTE_X_OFFSET = -4;
    private const int REMOTE_Z_OFFSET = -3;
    

    private void Start()
    {
        _addBetButton.onClick.AddListener(() =>
        {
            EventsHandler.Instance.PlayerPlacedBet(BET_INCREMENT);
        });
        
        _removeBetButton.onClick.AddListener(() =>
        {
            EventsHandler.Instance.PlayerRemovedBet(BET_INCREMENT);
        });

        EventsHandler.Instance.OnPlayerPlacedBet += PlaceBet;
        EventsHandler.Instance.OnPlayerRemovedBet += RemoveBet;
        EventsHandler.Instance.OnRemotePlayerPlacedBet += RemotePlayerPlacedBet;
        GenerateStacks(LOCAL_X_OFFSET, LOCAL_Z_OFFSET, _localBettingStacks);
        GenerateStacks(REMOTE_X_OFFSET, REMOTE_Z_OFFSET, _remoteBettingStacks);
    }
    
    private void GenerateStacks(int xOffset, int zOffset, List<Stack> _bettingStacks)
    {
        for (int i = 0; i < MAX_STACKS_LIMIT; i++)
        {
            var stack = Instantiate(_stack, new Vector3( _xSpace * (i % _rowLength) - xOffset, 
                    _stack.transform.position.y , _zSpace * (i / _rowLength) - zOffset)
                , Quaternion.identity);
            
            stack.Init();
            _bettingStacks.Add(stack);
        } 
    }
    
    private void PlaceBet(int amount)
    {
        if (_currentLocalStack < MAX_STACKS_LIMIT )
        {
            _localBetPlacedAmount += amount;
            UpdateVisual();
            _localBettingStacks[_currentLocalStack].gameObject.SetActive(false);
            _currentLocalStack++;
            EventsHandler.Instance.BetAmountChanged(_localBetPlacedAmount);
        }
    }
    
    private void RemoveBet(int amount)
    {
        if (_currentLocalStack > 0 )
        {
            _localBetPlacedAmount -= amount;
            UpdateVisual();
            _currentLocalStack--;
            _localBettingStacks[_currentLocalStack].gameObject.SetActive(true);
            EventsHandler.Instance.BetAmountChanged(_localBetPlacedAmount);
        }
    }
    
    private void RemotePlayerPlacedBet()
    {
        if (_currentRemoteStack < MAX_STACKS_LIMIT )
        {
            _remoteBettingStacks[_currentRemoteStack].gameObject.SetActive(false);
            _currentRemoteStack++;
        }
    }

    private void UpdateVisual()
    {
        _betAmountTxt.text = _localBetPlacedAmount.ToString();
    }

    private void OnDestroy()
    {
        EventsHandler.Instance.OnPlayerPlacedBet -= PlaceBet;
        EventsHandler.Instance.OnPlayerPlacedBet -= RemoveBet;
        EventsHandler.Instance.OnRemotePlayerPlacedBet += RemotePlayerPlacedBet;
    }
}
