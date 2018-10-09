using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerBehavior : NetworkBehaviour
{
    Timer timer;
    void Awake()
    {
        Debug.Log("this is a player");
        NetworkServer.Spawn(this.gameObject);
        timer = GetComponent<Timer>();

        if (isServer)
        {
            Debug.Log("This is server");
        }
        else
        {
            Debug.Log("This is not server");
        }
    }

    public void OnStartGame()
    {
        Debug.Log("from player behavior: game start!");
        timer.OnCountDown();
    }
}
