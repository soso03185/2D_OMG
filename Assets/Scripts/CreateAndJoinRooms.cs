using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public InputField createInput;
    public InputField joinInput;
    public GameObject roomNamePrefab; // 텍스트 프리팹
    public Transform roomListContainer; // 스크롤 뷰의 Content 오브젝트
    public TextMeshProUGUI m_RegionInfo;

    private List<GameObject> roomNameObjects = new List<GameObject>(); // 동적으로 생성된 텍스트 오브젝트 관리

    public void Start()
    {
        m_RegionInfo.text = $"region : {PhotonNetwork.CloudRegion}";
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("로비에 성공적으로 접속했습니다!");

        RefreshRoomList();
    }
     

    private void RefreshRoomList()
    {
        // 특정 조건을 사용하지 않으려면 null을 전달하면 됩니다.
        TypedLobby lobby = TypedLobby.Default; // 기본 로비를 사용
        string sqlLobbyFilter = null;  // 필터링 없이 모든 방을 요청

        PhotonNetwork.GetCustomRoomList(lobby, sqlLobbyFilter);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(createInput.text))
        {
            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = 4,
                IsVisible = true,
                IsOpen = true
            };
            Debug.Log($"Creating Room: {createInput.text}");
            PhotonNetwork.CreateRoom(createInput.text, roomOptions);
        }
    }

    public void JoinRoom()
    {
        if (!string.IsNullOrEmpty(joinInput.text))
        {
            Debug.Log($"Photon Network 상태: {PhotonNetwork.NetworkClientState}");
            Debug.Log($"Joining Room: {joinInput.text}");
            PhotonNetwork.JoinRoom(joinInput.text);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 기존의 방 이름 UI 제거
        foreach (GameObject roomObject in roomNameObjects)
        {
            Destroy(roomObject);
        }
        roomNameObjects.Clear();

        Debug.Log("Room List Update");

        // 새로운 방 이름 UI 생성
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList) continue; // 방이 삭제된 경우 무시

            GameObject roomNameObject = Instantiate(roomNamePrefab, roomListContainer);
            Room roomComponent = roomNameObject.GetComponent<Room>();
            Button roomBtn = roomNameObject.GetComponent<Button>();
            TextMeshProUGUI roomNameText = roomComponent.m_RoomName;


            roomNameText.text = room.Name; // 방 이름 표시
            roomNameObjects.Add(roomNameObject);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패: {message} (코드: {returnCode})");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 참여에 실패했습니다. ReturnCode: {returnCode}, Message: {message}");
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }
}