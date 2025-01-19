using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public TMP_InputField m_PlayerName;
    public TextMeshProUGUI m_InfoText;

    public void ConnectButton()
    {
        if (!string.IsNullOrEmpty(m_PlayerName.text))
        {
            // 로비에 접속할 때 유저 이름 등록
            PhotonNetwork.LocalPlayer.NickName = m_PlayerName.text; // + Random.Range(1000, 9999);
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        m_InfoText.text = "Connect to Server !";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            m_InfoText.text = "Move to Lobby Scene.";
            Debug.Log("로비에 성공적으로 접속했습니다!");

            Debug.Log($"현재 연결된 지역: {PhotonNetwork.CloudRegion}");
            Debug.Log($"현재 유저 이름: {PhotonNetwork.LocalPlayer.NickName}");
            SceneManager.LoadScene("LobbyScene");
        }
        else
        {
            m_InfoText.text = "PhotonNetwork Not Ready.";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("로비에 있는 방 리스트를 업데이트합니다.");
        foreach (var room in roomList)
        {
            Debug.Log($"방 이름: {room.Name}, 현재 인원: {room.PlayerCount}/{room.MaxPlayers}");
        }
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.LogError($"Photon 연결이 끊어졌습니다: {cause}");
    }

}
