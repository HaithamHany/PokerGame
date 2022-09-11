using System;
using System.Collections;
using System.Collections.Generic;
using Photon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectingHandler : PunBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private string _userId;
    [SerializeField] private string _previousRoom;

    private string _previousRoomPlayerPrefKey = "prevRoom";
    private const string _mainSceneName = "MainScene";

    const string NICK_NAME_KEY = "NickName";

    private void Start()
    {
        _inputField.text = PlayerPrefs.HasKey(NICK_NAME_KEY) ? 
            PlayerPrefs.GetString(NICK_NAME_KEY) : 
            string.Empty;
    }
    
    public void Connect()
    {
        var nickName = "Player00";
        if (_inputField != null && !string.IsNullOrEmpty(_inputField.text))
        {
            nickName = _inputField.text;
            PlayerPrefs.SetString(NICK_NAME_KEY, nickName);
        }
        
        if (PhotonNetwork.AuthValues == null)
        {
            PhotonNetwork.AuthValues = new AuthenticationValues();
        }
        
        PhotonNetwork.AuthValues.UserId = nickName;

        Debug.Log("Nickname: " + nickName + " userID: " + _userId, this);
        
        PhotonNetwork.playerName = nickName;
        PhotonNetwork.ConnectUsingSettings("0.5");
        
        PhotonHandler.StopFallbackSendAckThread();
    }
    
    public override void OnConnectedToMaster()
    {
        // after connect 
        _userId = PhotonNetwork.player.UserId;

        if (PlayerPrefs.HasKey(_previousRoomPlayerPrefKey))
        {
            Debug.Log("getting previous room from prefs: ");
            _previousRoom = PlayerPrefs.GetString(_previousRoomPlayerPrefKey);
            PlayerPrefs.DeleteKey(_previousRoomPlayerPrefKey); // we don't keep this, it was only for initial recovery
        }
        
        // after timeout: re-join "old" room (if one is known)
        if (!string.IsNullOrEmpty(this._previousRoom))
        {
            Debug.Log("ReJoining previous room: " + this._previousRoom);
            PhotonNetwork.ReJoinRoom(this._previousRoom);
            this._previousRoom = null;       // we only will try to re-join once. if this fails, we will get into a random/new room
        }
        else
        {
            // else: join a random room
            PhotonNetwork.JoinRandomRoom();
        }
    }
    
    public override void OnJoinedLobby()
    {
        OnConnectedToMaster(); // this way, it does not matter if we join a lobby or not
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        Debug.Log("OnPhotonRandomJoinFailed");
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2, PlayerTtl = 20000 }, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.room.Name);
        _previousRoom = PhotonNetwork.room.Name;
        PlayerPrefs.SetString(_previousRoom, _previousRoom);

    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        Debug.Log("OnPhotonJoinRoomFailed");
        _previousRoom = null;
        PlayerPrefs.DeleteKey(_previousRoomPlayerPrefKey);
    }

    public override void OnConnectionFail(DisconnectCause cause)
    {
        Debug.Log("Disconnected due to: " + cause + ". this.previousRoom: " + _previousRoom);
    }
	
    public override void OnPhotonPlayerActivityChanged(PhotonPlayer otherPlayer)
    {
        Debug.Log("OnPhotonPlayerActivityChanged() for "+otherPlayer.NickName+" IsInactive: "+otherPlayer.IsInactive);
    }
}
