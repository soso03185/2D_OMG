using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameScene : MonoBehaviour
{
    public GameObject playerPrefab;
    public PhotonView PV;

    void Start()
    {        
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);

        // 생성된 플레이어 오브젝트의 PhotonView 가져오기
        PhotonView PV = player.GetComponent<PhotonView>();

        // 로컬 플레이어와 원격 플레이어의 이름 설정
        if (PV.IsMine)
        {
            // 로컬 플레이어의 닉네임 설정
            player.GetComponent<PlayerController>()._playerName.text = PhotonNetwork.LocalPlayer.NickName;
        }
        else
        {
            // 원격 플레이어의 닉네임 설정
            player.GetComponent<PlayerController>()._playerName.text = PV.Owner.NickName;
        }
    }
}
