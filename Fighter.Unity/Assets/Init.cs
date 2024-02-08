using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
    public GameManager gm;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        NetworkClient.UID = "123456";
        NetworkClient.Token = "123456";
        Debug.Log("Connecting");
        NetworkClient.Connect("127.0.0.1", 20060);
        int id = gm.AddEntity(NetworkClient.UID, "Player", player);
        gm.FindEntityByID(id).AddComponent<NetworkTransform>();
        Debug.Log(gm.FindEntityByID(id).components.Count);
    }
}
