using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TMP_Text roomName;
    [SerializeField] TMP_Text online;

    public void SetInfo(RoomInfo info)
    {
        roomName.text = info.Name;
        online.text = $"{info.PlayerCount}/{info.MaxPlayers}";
    }

    public void Connect()
    {
        PhotonNetwork.JoinRoom(roomName.text);
    }
}
