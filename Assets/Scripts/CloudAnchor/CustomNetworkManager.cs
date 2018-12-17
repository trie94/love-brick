namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;
    using System;

    public class CustomNetworkManager : NetworkManager
    {
        [SerializeField] CloudAnchorController cloudAnchorController;
        public override void OnServerConnect(NetworkConnection conn)
        {
            Debug.Log("server connected: " + conn);
            base.OnServerConnect(conn);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            Debug.Log("On client connected");
            base.OnClientConnect(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            Debug.Log("Client disconnected: " + conn.lastError);
            base.OnServerDisconnect(conn);
        }
    }
}