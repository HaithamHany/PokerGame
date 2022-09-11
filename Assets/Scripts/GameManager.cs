using System;
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
    [SerializeField] private float _xOffset;
    [SerializeField] private float _zOffset;

    [Header("Bet Handling")] [Space] 
    [SerializeField] private TextMeshProUGUI _betAmountTxt;
    [SerializeField] private Button _addBetButton;
    [SerializeField] private Button _removeBetButton;
    
    //betting stacks
    private List<Stack> _bettingStacks = new List<Stack>();
    private int _betPlacedAmount;
    private int _currentStack = 0;
    
    private const int MAX_STACKS_LIMIT = 10;
    private const int BET_INCREMENT = 10;
    

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
        GenerateStacks();
    }
    
    private void GenerateStacks()
    {
        for (int i = 0; i < MAX_STACKS_LIMIT; i++)
        {
            var stack = Instantiate(_stack, new Vector3( _xSpace * (i % _rowLength) -_xOffset, 
                    _stack.transform.position.y , _zSpace * (i / _rowLength) - _zOffset)
                , Quaternion.identity);
            
            stack.Init();
            _bettingStacks.Add(stack);
        } 
    }
    
    private void PlaceBet(int amount)
    {
        if (_currentStack < MAX_STACKS_LIMIT )
        {
            _betPlacedAmount += amount;
            UpdateVisual();
            _bettingStacks[_currentStack].gameObject.SetActive(false);
            _currentStack++;
        }
    }
    
    private void RemoveBet(int amount)
    {
        if (_currentStack > 0 )
        {
            _betPlacedAmount -= amount;
            UpdateVisual();
            _currentStack--;
            _bettingStacks[_currentStack].gameObject.SetActive(true);
        }
    }

    private void UpdateVisual()
    {
        _betAmountTxt.text = _betPlacedAmount.ToString();
    }

    private void OnDestroy()
    {
        EventsHandler.Instance.OnPlayerPlacedBet -= PlaceBet;
        EventsHandler.Instance.OnPlayerPlacedBet -= RemoveBet;
    }
}
