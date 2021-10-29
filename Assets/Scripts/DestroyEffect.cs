using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEffect : MonoBehaviour
{    void Start()
    {
        StartCoroutine(DestroyAfter(this.gameObject, 0.7f));
    }

    IEnumerator DestroyAfter(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(obj);
    }

}
