using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon;
using TMPro;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Handling multiplayer aspect of the game using Photon engine
/// </summary>
public class MultiplayerManager : PunBehaviour, IPunTurnManagerCallbacks
{
    [SerializeField] private RectTransform _connectUiView;
    [SerializeField] private TextMeshProUGUI _remoteBetDisplay;
    [SerializeField] private TextMeshProUGUI _localBetDisplay;
    [SerializeField] private TextMeshProUGUI _remotePlayerNameDisplay;
    [SerializeField] private TextMeshProUGUI _localPlayerNameDisplay;
    [SerializeField] private TextMeshProUGUI _timerDisplayText;
    [SerializeField] private CanvasGroup _bettingPanelCanvasGroup;
    [SerializeField] private RectTransform _disconnectedPanel;
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private GameObject _rotatingBetObject;
    [SerializeField] private GameObject _greenBetObject;
    [SerializeField] private GameObject _redBetObject;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private GameObject _remoteBetPlacedImage;
    [SerializeField] private GameObject _localBetPlacedImage;
    [SerializeField] private TextMeshProUGUI _totalEarningsText;
    
    private PunTurnManager _turnManager;
    private float _timeCounter;
    private bool _isShowingResults;
    private int _remoteBet;
    private int _totalEarnings;
    private int _accumilatedEarnings;
    private EBetObjectSide _localSelection;
    private EBetObjectSide _remoteSelection;
    private EBetObjectSide _objectSide;
    private EBetObjectSide _remoteCoinSide;
    private EBetObjectSide _localCoinSide;
    private EResult _result;

    private readonly byte SendRandomResult = 0;
    private readonly byte BetAmountUpdated = 10;

    private const float WAIT_START_TOSS_TIME = 4;
    private const float WAIT_END_TOSS_TIME = 2;
    private const int MAX_PLAYERS_COUNT = 2;
    
    private void Start()
    {
        _turnManager = gameObject.AddComponent<PunTurnManager>();
        _turnManager.TurnManagerListener = this;
        _turnManager.TurnDuration = 190f;
        
        _remoteBetDisplay.gameObject.SetActive(false);
        RefreshUIViews();
        EventsHandler.Instance.OnBetAmountChanged += BetAmountChanged;
        EventsHandler.Instance.OnPlayerPlacedBet += LocalBetPlaced;
    }

    private void Update()
    {
        if (!PhotonNetwork.inRoom) return;

        UpdateCoreGame();
        UpdatePlayerVisuals();
        
        // remote player's bet  is shown when the turn is complete
        if (_turnManager.IsCompletedByAll)
        {
            //Show remote bet 
            _remoteBetDisplay.text = _remoteBet.ToString();
            Debug.Log("Flip betting object...");
        }
        else
        {
           FullTurnIncomplete();
        }
    }

    private void FullTurnIncomplete()
    {
        _bettingPanelCanvasGroup.interactable = PhotonNetwork.room.PlayerCount > 1;
            
        if (_turnManager.Turn > 0 && !_turnManager.IsCompletedByAll)
        {
            // alpha of the remote text is used to show if the remote player active or had made a turn
            var remote = PhotonNetwork.player.GetNext();
            var alpha = 0.5f;
            
            if (_turnManager.GetPlayerFinishedTurn(remote))
            {
                alpha = 1;
            }
            
            if (remote != null && remote.IsInactive)
            {
                alpha = 0.1f;
            }

            _remotePlayerNameDisplay.color = new Color(1, 1, 1, alpha);
        }
    }

    private void UpdateCoreGame()
    {
        if (PhotonNetwork.connected && _disconnectedPanel.gameObject.activeInHierarchy)
        {
            _disconnectedPanel.gameObject.SetActive(false);
        }
        
        if (!PhotonNetwork.connected && !PhotonNetwork.connecting && !_disconnectedPanel.gameObject.activeInHierarchy)
        {
            _disconnectedPanel.gameObject.SetActive(true);
        }
        
        if (PhotonNetwork.room.PlayerCount>1)
        {
            if (_turnManager.IsOver) return;

            if (_turnText != null)
            {
                _turnText.text = _turnManager.Turn.ToString();
            }

            if (_turnManager.Turn > 0 && _timerDisplayText != null && ! _isShowingResults)
            {
                _timerDisplayText.text = _turnManager.RemainingSecondsInTurn.ToString("F1");
            }
        }
    }
    
    private void RefreshUIViews()
    {
        _timeCounter = 0;
        _timerDisplayText.text = _timeCounter.ToString();

        _connectUiView.gameObject.SetActive(!PhotonNetwork.inRoom);

        _bettingPanelCanvasGroup.interactable = PhotonNetwork.room != null && PhotonNetwork.room.PlayerCount > 1;
    }
    
    private void UpdatePlayerVisuals()
    {
        var remote = PhotonNetwork.player.GetNext();
        var local = PhotonNetwork.player;

        _remotePlayerNameDisplay.text = (remote != null)?  
            $"{remote.NickName}  {remote.GetScore():D2}" :
            "waiting for another player        00";
        
        if (remote == null)
        {
            _timerDisplayText.text = "";
        }
        
        if (local != null)
        {
            _localPlayerNameDisplay.text = $"YOU      {local.GetScore():D2}";
        }
    }

    public void OnTurnBegins(int turn)
    {
        Debug.Log("OnTurnBegins() turn: "+ turn);
        EventsHandler.Instance.TurnStarted();
        _totalEarnings = 0;
        _localBetPlacedImage.SetActive(false);
        _remoteBetPlacedImage.SetActive(false);
        _rotatingBetObject.gameObject.SetActive(false);
        _greenBetObject.SetActive(false);
        _redBetObject.SetActive(false);
        _localSelection = EBetObjectSide.None;
        _remoteSelection = EBetObjectSide.None;
        _resultText.gameObject.SetActive(false);
        
        _isShowingResults = false;
        _bettingPanelCanvasGroup.interactable = true;
    }

    public void OnTurnCompleted(int turn)
    {
        Debug.Log("OnTurnCompleted: " + turn);

        EvaluateBetResult();
        UpdateScores();
        OnEndTurn();
    }
    

    public void OnPlayerMove(PhotonPlayer player, int turn, object move)
    {
        Debug.Log($"OnPlayerMove: {player} turn: {turn} action: {move}");
        throw new System.NotImplementedException();
    }

    public void OnPlayerFinished(PhotonPlayer player, int turn, object move)
    {
        Debug.Log($"OnTurnFinished: {player} turn: {turn} action: {move}");

        if (player.IsLocal)
        {
            _localSelection = (EBetObjectSide)(byte)move;
            _localBetPlacedImage.SetActive(true);
        }
        else
        {
            _remoteSelection = (EBetObjectSide)(byte)move;
            _remoteBetPlacedImage.SetActive(true);
        }
    }

    public void OnTurnTimeEnds(int turn)
    {
        if (!_isShowingResults)
        {
            Debug.Log("OnTurnTimeEnds: Calling OnTurnCompleted");
            OnTurnCompleted(-1);
        }
    }
    
    private void EvaluateBetResult()
    {
        _result = EResult.Draw;

        if (_localSelection == _remoteSelection) return;
        
        if (_localSelection == EBetObjectSide.None)
        {
            _result = EResult.Loss;
            return;
        }

        if (_remoteSelection == EBetObjectSide.None) _result = EResult.Win;

        _result = (_localSelection == _objectSide) ? EResult.Win : EResult.Loss;
    }
    
    private void UpdateScores()
    {
        if (_result == EResult.Win)
        {
            PhotonNetwork.player.AddScore(1);
        }
    }
    
    public void StartTurn()
    {
        if (PhotonNetwork.isMasterClient)
        {
            //Generate game results for all clients
            GenerateGameResult(); 
            _turnManager.BeginTurn();
        }
    }
    
    public void MakeTurn(EBetObjectSide selection)
    {
        _turnManager.SendMove((byte)selection, true);
    }
    
    public void OnEndTurn()
    {
        StartCoroutine(ShowResultsAndStartNextTurn());
    }
    
    private IEnumerator ShowResultsAndStartNextTurn()
    {
        _bettingPanelCanvasGroup.interactable = false;
        _isShowingResults = true;

        _rotatingBetObject.SetActive(true);
        yield return Toss();
        
        UpdateResultsVisual();
        
        var local = PhotonNetwork.player;
        if (local != null && _result == EResult.Win)
        {
            _totalEarningsText.text = $"Total Earnings: ${_accumilatedEarnings += _totalEarnings}";
        }
        _resultText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2.0f);

        StartTurn();
    }

    private void UpdateResultsVisual()
    {
        if (_result == EResult.Draw)
        {
            _resultText.text = $"DRAW!";
        }
        else
        {
            _resultText.text = _result == EResult.Win ? $"YOU WIN! ${_totalEarnings}" : "YOU LOSE!";
        }
    }
    
    private IEnumerator Toss()
    {
        yield return new WaitForSeconds(WAIT_START_TOSS_TIME);
        _rotatingBetObject.SetActive(false);
        _greenBetObject.SetActive(_objectSide == EBetObjectSide.Green);
        _redBetObject.SetActive(_objectSide == EBetObjectSide.Red);

        yield return new WaitForSeconds(WAIT_END_TOSS_TIME);
    }
    
    #region Buttons Handler
   
    public void OnClickRedSide()
    {
        MakeTurn(EBetObjectSide.Red);
    }

    public void OnClickGreenSide()
    {
        MakeTurn(EBetObjectSide.Green);
    }
    
    public void OnClickReConnectAndRejoin()
    {
        PhotonNetwork.ReconnectAndRejoin();
        PhotonHandler.StopFallbackSendAckThread();  
    }
    
    
    public override void OnLeftRoom()
    {
        Debug.Log("Player left room");
        RefreshUIViews();
    }
    #endregion
    
    private void GenerateGameResult()
    {
        var randomTossSelection = UnityEngine.Random.Range(1, 3);
        _objectSide = (EBetObjectSide) randomTossSelection;
        
        var content = new object[] { _objectSide };
        var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; 
        PhotonNetwork.RaiseEvent(SendRandomResult, content,true, raiseEventOptions);
    }
    
    public void OnEvent(byte eventCode, object content, int senderId)
    {
        if (eventCode == SendRandomResult)
        {
            var data = (object[])content;
            var coinSide = (EBetObjectSide)data[0];
            _objectSide = coinSide;
        }

        if (eventCode == BetAmountUpdated)
        {
            var data = (object[])content;
            var amount = (int)data[0];
            UpdateRemoteVisual(amount);
            _totalEarnings += 10;
            EventsHandler.Instance.RemotePlayerPlacedBet();
        }
    }
    
    public override void OnJoinedRoom()
    {
        RefreshUIViews();
        CheckBeforeStartingTurn();
    }
    
    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("Other player has arrived");

        CheckBeforeStartingTurn();
    }

    private void CheckBeforeStartingTurn()
    {
        if (PhotonNetwork.room.PlayerCount == MAX_PLAYERS_COUNT && _turnManager.Turn == 0)
        {
            // when the room has two players, start the first turn
            StartTurn();
            return;
        }
        
        Debug.Log("Waiting Player");
    }
    
    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        Debug.Log($"Other player disconnected! {otherPlayer.ToStringFull()}");
    }
    
    public override void OnConnectionFail(DisconnectCause cause)
    {
        _disconnectedPanel.gameObject.SetActive(true);
    }
    
    
    private void LocalBetPlaced(int amout)
    {
        _totalEarnings += amout;
    }

    private void BetAmountChanged(int amount)
    {
        var content = new object[] { amount };
        var raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others}; 
        PhotonNetwork.RaiseEvent(BetAmountUpdated, content,true , raiseEventOptions);
    }

    private void UpdateRemoteVisual(int betAmount)
    {
        _remoteBetDisplay.gameObject.SetActive(true);
        _remoteBetDisplay.text = $"${betAmount.ToString()}";
    }
    
    public void OnEnable()
    {
        PhotonNetwork.OnEventCall += OnEvent;
    }

    public void OnDisable()
    {
        PhotonNetwork.OnEventCall -= OnEvent;
        EventsHandler.Instance.OnBetAmountChanged -= BetAmountChanged;
        EventsHandler.Instance.OnPlayerPlacedBet -= LocalBetPlaced;
    }
}

public enum EResult
{
    None = 0,
    Win = 1,
    Loss = 2,
    Draw = 3
}

public enum EBetObjectSide
{
    None = 0,
    Red = 1,
    Green = 2,
}

