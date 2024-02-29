using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UniRx;
using EasyButtons;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    //[SerializeField] private string region = "ru";
    public Subject<List<RoomInfo>> roomList = new Subject<List<RoomInfo>>();
    public ReactiveProperty<bool> isJoinedLobby;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public static void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void CreateRoom(TMP_InputField input)
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 5;
        options.IsVisible = true;
        options.IsOpen = true;
        PhotonNetwork.CreateRoom(input.text, options, TypedLobby.Default, null);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log($"Joined lobby");
        isJoinedLobby.Value = true;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log($"Connected to: {PhotonNetwork.CloudRegion}");
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected from server.");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log($"Created room: {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log($"Create room failed: {message}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined a room successfully");
        GameManager.Instance?.Play();
    }

    public override void OnLeftRoom()
    {
        Debug.Log($"Room exit");
        GameManager.Instance?.Exit();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogErrorFormat("Room creation failed with error code {0} and error message {1}", returnCode, message);
        isJoinedLobby.Value = false;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"RoomList recieved");
        this.roomList.OnNext(roomList);
    }
}
