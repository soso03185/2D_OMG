using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public Text StatusText;
    public InputField roomInput, NickNameInput;

    void Awake() => Screen.SetResolution(960, 540, false);
    private void Start() { PhotonNetwork.ConnectUsingSettings(); print("서버접속완료"); }
    void Update() => StatusText.text = PhotonNetwork.NetworkClientState.ToString();

    public void CreateRoom() => PhotonNetwork.CreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 4 });
    public override void OnCreatedRoom() => print("방만들기-완료");

    public void JoinRoom() => PhotonNetwork.JoinRoom(roomInput.text);
    public override void OnJoinedRoom() 
    {
            print("방참가완료");
            PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
            PhotonNetwork.LoadLevel("GameScene");
    }



    //public void Connect() => PhotonNetwork.ConnectUsingSettings();
    //public override void OnConnectedToMaster() => print("서버접속완료");

    //public void Disconnect() => PhotonNetwork.Disconnect();
    //public override void OnDisconnected(DisconnectCause cause) => print("연결끊김");


    //public void JoinLobby() => PhotonNetwork.JoinLobby();
    //public override void OnJoinedLobby() => print("로비접속완료");
    //public void JoinOrCreateRoom() => PhotonNetwork.JoinOrCreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 2 }, null);

    //public override void OnCreateRoomFailed(short returnCode, string message) => print("방만들기실패");
    //public override void OnJoinRoomFailed(short returnCode, string message) => print("방참가실패");
    //public override void OnJoinRandomFailed(short returnCode, string message) => print("방랜덤참가실패");
}
