namespace Love.Core
{
    using System;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// Network Client that resolves rooms to anchor id on other devices.
    /// </summary>
    public class RoomSharingClient
    {
        /// <summary>
        /// The callback to call after the anchor id is received.
        /// </summary>
        private GetAnchorIdFromRoomDelegate m_GetAnchorIdFromRoomCallback;

        /// <summary>
        /// The room id to resolve.
        /// </summary>
        private int m_RoomId;

        /// <summary>
        /// Get anchor identifier from room delegate.
        /// </summary>
        /// <param name="found">Tells if the Anchor id was found or not.</param>
        /// <param name="anchorId">The anchor id of the room.</param>
        public delegate void GetAnchorIdFromRoomDelegate(bool found, string anchorId);

        /// <summary>
        /// Gets the anchor id of a room.
        /// </summary>
        /// <param name="roomId">Room identifier to resolve.</param>
        /// <param name="ipAddress">The Ip address of the device where the room belongs to.</param>
        /// <param name="GetAnchorIdFromRoomCallback">The callback to be called after the room was resolved.</param>
        public void GetAnchorIdFromRoom(Int32 roomId, string ipAddress, GetAnchorIdFromRoomDelegate GetAnchorIdFromRoomCallback)
        {
            m_GetAnchorIdFromRoomCallback = GetAnchorIdFromRoomCallback;
            m_RoomId = roomId;
            NetworkManager.singleton.client.RegisterHandler(MsgType.Connect, OnConnected);
            NetworkManager.singleton.client.RegisterHandler(RoomSharingMsgType.AnchorIdFromRoomResponse, OnGetAnchorIdFromRoomResponse);
            NetworkManager.singleton.client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
            NetworkManager.singleton.client.RegisterHandler(MsgType.Error, OnError);
            // NetworkManager.singleton.networkAddress = ipAddress;
            // NetworkManager.singleton.networkPort = 8888;
            // NetworkManager.singleton.StartClient();
            Debug.Log("get anchor id from room");
            // NetworkManager.singleton.client.Connect(ipAddress, 8888);
        }

        /// <summary>
        /// Handles connected event.
        /// </summary>
        /// <param name="networkMessage">The Connect response.</param>
        private void OnConnected(NetworkMessage networkMessage)
        {
            Debug.Log("On connected from room shring client");
            AnchorIdFromRoomRequestMessage anchorIdRequestMessage = new AnchorIdFromRoomRequestMessage
            {
                RoomId = m_RoomId
            };
            NetworkManager.singleton.client.Send(RoomSharingMsgType.AnchorIdFromRoomRequest, anchorIdRequestMessage);
        }

        /// <summary>
        /// Handles when there is an error connecting to the server.
        /// </summary>
        /// <param name="networkMessage">Error message.</param>
        private void OnError(NetworkMessage networkMessage)
        {
            Debug.Log("Error connecting to Room Sharing Server");
            if (m_GetAnchorIdFromRoomCallback != null)
            {
                m_GetAnchorIdFromRoomCallback(false, null);
            }
            Debug.Log("On error");
        }

        /// <summary>
        /// Handles when there is disconnection from the server.
        /// </summary>
        /// <param name="networkMessage">Disconnection message.</param>
        private void OnDisconnected(NetworkMessage networkMessage)
        {
            Debug.Log("Disconnected from Room Sharing Server");
            if (m_GetAnchorIdFromRoomCallback != null)
            {
                m_GetAnchorIdFromRoomCallback(false, null);
            }
        }

        /// <summary>
        /// Handles the resolve room response from server.
        /// </summary>
        /// <param name="networkMessage">The resolve room response message.</param>
        private void OnGetAnchorIdFromRoomResponse(NetworkMessage networkMessage)
        {
            var response = networkMessage.ReadMessage<AnchorIdFromRoomResponseMessage>();
            if (m_GetAnchorIdFromRoomCallback != null)
            {
                m_GetAnchorIdFromRoomCallback(response.Found, response.AnchorId);
            }

            m_GetAnchorIdFromRoomCallback = null;
            Debug.Log("on get anchor id from room response: " + response.AnchorId);
        }
    }
}