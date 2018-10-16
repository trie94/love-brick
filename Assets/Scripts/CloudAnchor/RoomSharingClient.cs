namespace Love.Core
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// Network Client that resolves rooms to anchor id on other devices.
    /// </summary>
    public class RoomSharingClient : NetworkClient
    {

        /// <summary>
        /// The callback to call after the anchor id is received.
        /// </summary>
        private GetAnchorIdFromRoomDelegate m_GetAnchorIdFromRoomCallback;

        /// <summary>
        /// The room id to resolve.
        /// </summary>
        private int m_RoomId;
        public float totalTime;

        /// <summary>
        /// Get anchor identifier from room delegate.
        /// </summary>
        /// <param name="found">Tells if the Anchor id was found or not.</param>
        /// <param name="anchorId">The anchor id of the room.</param>
        public delegate void GetAnchorIdFromRoomDelegate(bool found, string anchorId);

        // timer
        TimerDelegate timerDelegate;
        public delegate void TimerDelegate(float time);
        public event TimerDelegate OnTimerReceived;

        // block spawner
        BlockSpawnerDelegate blockSpawnerDelegate;
        public delegate void BlockSpawnerDelegate(Vector3 position, Quaternion rotation);
        public event BlockSpawnerDelegate OnSpawnerReady;
        

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
            RegisterHandler(MsgType.Connect, OnConnected);
            RegisterHandler(RoomSharingMsgType.AnchorIdFromRoomResponse, OnGetAnchorIdFromRoomResponse);
            RegisterHandler(MsgType.Disconnect, OnDisconnected);
            RegisterHandler(MsgType.Error, OnError);

            // custom handler
            RegisterHandler(RoomSharingMsgType.timer, OnTimerResponse);
            RegisterHandler(RoomSharingMsgType.blockSpawner, OnSpawnerResponse);

            // NetworkManager.singleton.networkAddress = ipAddress;
            // NetworkManager.singleton.networkPort = 8888;
            // NetworkManager.singleton.StartClient();
            Connect(ipAddress, 8888);
        }

        /// <summary>
        /// Handles connected event.
        /// </summary>
        /// <param name="networkMessage">The Connect response.</param>
        private void OnConnected(NetworkMessage networkMessage)
        {
            AnchorIdFromRoomRequestMessage anchorIdRequestMessage = new AnchorIdFromRoomRequestMessage
            {
                RoomId = m_RoomId
            };
            Send(RoomSharingMsgType.AnchorIdFromRoomRequest, anchorIdRequestMessage);
        }

        /// <summary>
        /// Handles when there is an error connecting to the server.
        /// </summary>
        /// <param name="networkMessage">Error message.</param>
        private void OnError(NetworkMessage networkMessage)
        {
            if (m_GetAnchorIdFromRoomCallback != null)
            {
                m_GetAnchorIdFromRoomCallback(false, null);
            }
        }

        /// <summary>
        /// Handles when there is disconnection from the server.
        /// </summary>
        /// <param name="networkMessage">Disconnection message.</param>
        private void OnDisconnected(NetworkMessage networkMessage)
        {
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

        // custom message
        void OnTimerResponse(NetworkMessage networkMessage)
        {
            var response = networkMessage.ReadMessage<TimerMessage>();
            Debug.Log("on timer response: " + response.totalTime);
            if (OnTimerReceived != null)
            {
                OnTimerReceived(response.totalTime);
            }
        }

        void OnSpawnerResponse(NetworkMessage networkMessage)
        {
            var response = networkMessage.ReadMessage<BlockSpawner>();
            // Debug.Log("on block response: " + response.blockPos);
            if(OnSpawnerReady != null)
            {
                OnSpawnerReady(response.blockPos, response.blockRot);
            }
        }
    }
}