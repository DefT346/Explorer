using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NetworkView : MonoBehaviour
{
    [SerializeField] private PhotonManager photonManager; 
    [SerializeField] private RectTransform content;
    [SerializeField] private RoomListItem roomItemPrefab;
    [SerializeField] private Button createRoomButton;

    void Awake()
    {
        photonManager.roomList.Subscribe(roomList =>
        {
            DrawRoomList(roomList);
        });

        photonManager.isJoinedLobby.Subscribe(isConnected =>
        {
            createRoomButton.interactable = isConnected;
        });
    }

    void DrawRoomList(List<RoomInfo> roomList)
    {
        ClearRoomList();
        foreach (var roomInfo in roomList)
        {
            if (roomInfo.MaxPlayers == 0) continue;
            RoomListItem listItem = Instantiate(roomItemPrefab, content);
            listItem.SetInfo(roomInfo);
        }
    }

    void ClearRoomList()
    {
        var trs = content.GetComponentsInChildren<RectTransform>();
        foreach (var tr in trs)
        {
            if (tr != content)
                Destroy(tr.gameObject);
        }
    }

}
