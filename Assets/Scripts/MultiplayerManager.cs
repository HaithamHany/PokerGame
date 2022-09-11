using System;
using System.Collections;
using System.Collections.Generic;
using Photon;
using TMPro;
using UnityEngine;
using Random = System.Random;

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
    
    private PunTurnManager _turnManager;
    private float _timeCounter;
    private bool _isShowingResults;
    private int _remoteBet;
    private int _LocalBetAmount;
    private EBetObjectSide _localSelection;
    private EBetObjectSide _remoteSelection;
    private EBetObjectSide _coinSide;
    private EResult _result;

    public int RemoteBet => _remoteBet;
        
    private void Start()
    {
        _turnManager = gameObject.AddComponent<PunTurnManager>();
        _turnManager.TurnManagerListener = this;
        _turnManager.TurnDuration = 30f;
        
        _remoteBetDisplay.gameObject.SetActive(false);
        _localBetDisplay .gameObject.SetActive(false);
        RefreshUIViews();
    }

    private void Update()
    {
        if (!PhotonNetwork.inRoom) return;
        
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
        
        UpdatePlayerVisuals();
        
        //Already happening locally by placing the bet
        // // show local player's selected hand
        // Sprite selected = SelectionToSprite(this.localSelection);
        // if (selected != null)
        // {
        //     this.localSelectionImage.gameObject.SetActive(true);
        //     this.localSelectionImage.sprite = selected;
        // }
        
        // remote player's selection is only shown, when the turn is complete (finished by both) then flip the coin
        if (_turnManager.IsCompletedByAll)
        {
            //Show remote bet then flip coin
            _remoteBetDisplay.text = _remoteBet.ToString();
            Debug.Log("Flip Coin...");
    
        }
        else
        {
            _bettingPanelCanvasGroup.interactable = PhotonNetwork.room.PlayerCount > 1;

            // if (PhotonNetwork.room.PlayerCount < 2)
            // {
            //     this.remoteSelectionImage.color = new Color(1, 1, 1, 0);
            // }

            // if the turn is not completed by all, we use a random image for the remote hand
            if (_turnManager.Turn > 0 && !_turnManager.IsCompletedByAll)
            {
                // alpha of the remote hand is used as indicator if the remote player "is active" and "made a turn"
                PhotonPlayer remote = PhotonNetwork.player.GetNext();
                float alpha = 0.5f;
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
        PhotonPlayer remote = PhotonNetwork.player.GetNext();
        PhotonPlayer local = PhotonNetwork.player;

        if (remote != null)
        {
            _remotePlayerNameDisplay.text = $"{remote.NickName}      {remote.GetScore():D2}";
        }
        else
        {
            _timerDisplayText.text = "";
            _remotePlayerNameDisplay.text = "waiting for another player        00";
        }
        
        if (local != null)
        {
            _localPlayerNameDisplay.text = $"YOU  {local.GetScore():D2}";
        }
    }

    public void OnTurnBegins(int turn)
    {
        Debug.Log("OnTurnBegins() turn: "+ turn);
        _rotatingBetObject.gameObject.SetActive(false);
        _greenBetObject.SetActive(false);
        _redBetObject.SetActive(false);
        _localSelection = EBetObjectSide.None;
        _remoteSelection = EBetObjectSide.None;

        _resultText.gameObject.SetActive(false);

        // this.localSelectionImage.gameObject.SetActive(false);
        // this.remoteSelectionImage.gameObject.SetActive(true);

        _isShowingResults = false;
        _bettingPanelCanvasGroup.interactable = true;
    }

    public void OnTurnCompleted(int turn)
    {
        Debug.Log("OnTurnCompleted: " + turn);

        CalculateWinAndLoss();
        UpdateScores();
        OnEndTurn();
    }
    

    public void OnPlayerMove(PhotonPlayer player, int turn, object move)
    {
        Debug.Log("OnPlayerMove: " + player + " turn: " + turn + " action: " + move);
        throw new System.NotImplementedException();
    }

    public void OnPlayerFinished(PhotonPlayer player, int turn, object move)
    {
        Debug.Log("OnTurnFinished: " + player + " turn: " + turn + " action: " + move);

        if (player.IsLocal)
        {
            _localSelection = (EBetObjectSide)(byte)move;
        }
        else
        {
            _remoteSelection = (EBetObjectSide)(byte)move;
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
    
    private void CalculateWinAndLoss()
    {
        _result = EResult.Draw;
        
        var randomTossSelection = UnityEngine.Random.Range(1, 2);
        _coinSide = (EBetObjectSide) randomTossSelection;
        
        if (_localSelection == _remoteSelection) return;
        
        if (_localSelection == EBetObjectSide.None)
        {
            _result = EResult.Loss;
            return;
        }

        if (_remoteSelection == EBetObjectSide.None) _result = EResult.Win;

        
        if (_localSelection == EBetObjectSide.None)
        {
            _result = (_remoteSelection == _coinSide) ? EResult.Win :  EResult.Loss;
        }
        if (_localSelection == _coinSide)
        {
            _result = (_remoteSelection != _coinSide) ? EResult.Win : EResult.Loss;
        }
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
            _turnManager.BeginTurn();
        }
    }
    
    public void MakeTurn(EBetObjectSide selection)
    {
        _turnManager.SendMove((byte)selection, true);
    }
    
    public void OnEndTurn()
    {
        StartCoroutine(ShowResultsBeginNextTurnCoroutine());
    }
    
    private IEnumerator ShowResultsBeginNextTurnCoroutine()
    {
        _bettingPanelCanvasGroup.interactable = false;
        _isShowingResults = true;
        // yield return new WaitForSeconds(1.5f);
        _rotatingBetObject.SetActive(true);
        yield return Toss();
        
        if (_result == EResult.Draw)
        {
            _resultText.text = $"DRAW!";
        }
        
        else
        {
            _resultText.text = _result == EResult.Win ? "YOU WIN!" : "YOU LOSE!";
        }
        _resultText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2.0f);

        StartTurn();
    }
    
    private IEnumerator Toss()
    {
        yield return new WaitForSeconds(4);
        _rotatingBetObject.SetActive(false);
        _greenBetObject.SetActive(_coinSide == EBetObjectSide.Green);
        _redBetObject.SetActive(_coinSide == EBetObjectSide.Red);

        yield return new WaitForSeconds(2);
    }
    
    // public IEnumerator CycleRemoteHandCoroutine()
    // {
    //     while (true)
    //     {
    //         // cycle through available images
    //         this.randomHand = (Hand)Random.Range(1, 4);
    //         yield return new WaitForSeconds(0.5f);
    //     }
    // }
    
    #region Handling Of Buttons
    //Handling using Events
    public void OnClickRedSide()
    {
        MakeTurn(EBetObjectSide.Red);
    }

    public void OnClickGreenSide()
    {
        this.MakeTurn(EBetObjectSide.Green);
    }
    
    public void OnClickConnect()
    {
        PhotonNetwork.ConnectUsingSettings(null);
        PhotonHandler.StopFallbackSendAckThread();  
    }
    
    public void OnClickReConnectAndRejoin()
    {
        PhotonNetwork.ReconnectAndRejoin();
        PhotonHandler.StopFallbackSendAckThread();  
    }

    #endregion
    
    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom()");
        RefreshUIViews();
    }
    
    public override void OnJoinedRoom()
    {
        RefreshUIViews();
        //This is redundant code
        if (PhotonNetwork.room.PlayerCount == 2)
        {
            if (_turnManager.Turn == 0)
            {
                // when the room has two players, start the first turn (later on, joining players won't trigger a turn)
                StartTurn();
            }
        }
        else
        {
            Debug.Log("Waiting for another player");
        }
    }
    
    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("Other player arrived");

        if (PhotonNetwork.room.PlayerCount == 2)
        {
            if (_turnManager.Turn == 0)
            {
                // when the room has two players, start the first turn (later on, joining players won't trigger a turn)
                this.StartTurn();
            }
        }
    }
    
    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        Debug.Log("Other player disconnected! "+otherPlayer.ToStringFull());
    }


    public override void OnConnectionFail(DisconnectCause cause)
    {
        _disconnectedPanel.gameObject.SetActive(true);
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

