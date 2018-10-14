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
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            Debug.Log("On client connected");
			// networkClient.Send();
        }

		public void OnConnected(Int32 m_RoomId)
        {
            AnchorIdFromRoomRequestMessage anchorIdRequestMessage = new AnchorIdFromRoomRequestMessage
            {
                RoomId = m_RoomId
            };

            // roomSharingClient.Send(RoomSharingMsgType.AnchorIdFromRoomRequest, anchorIdRequestMessage);
            Debug.Log("[custom network manager] On connected: " + m_RoomId);
        }

    }
}