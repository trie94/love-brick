namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;
	using System;

    public class CustomNetworkManager : NetworkManager
    {
		RoomSharingClient roomSharingClient;

        public override void OnServerConnect(NetworkConnection conn)
        {
            Debug.Log("server connected");
            base.OnServerConnect(conn);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            Debug.Log("On client connected");
            base.OnClientConnect(conn);
			// roomSharingClient.Send();
        }
    }
}