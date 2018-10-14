namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;
    using TMPro;
    using GoogleARCore;

    public class GameController : NetworkBehaviour
    {
        [SerializeField] NetworkManager networkManager;
		
        void Start()
        {
            if (!isServer)
            {
                this.enabled = false;
            }
            else
            {
                Debug.Log("game controller: this is server");
            }
        }
    }
}