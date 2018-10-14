namespace Love.Core
{
    using System;
    using System.Collections.Generic;
    using GoogleARCore.CrossPlatform;
    using UnityEngine;
    using UnityEngine.Networking;

    /// <summary>
    /// Server to share rooms to other devices.
    /// </summary>
    public class RoomSharingServer : MonoBehaviour
    {
        /// <summary>
        /// The dictionary that maps local rooms to hosted Anchors.
        /// </summary>
        private Dictionary<int, XPAnchor> m_RoomAnchorsDict = new Dictionary<int, XPAnchor>();

        /// <summary>
        /// Initialize the server.
        /// </summary>
        public void Start()
        {
            NetworkServer.Listen(8888);
            NetworkServer.RegisterHandler(RoomSharingMsgType.AnchorIdFromRoomRequest, OnGetAnchorIdFromRoomRequest);
        }

        /// <summary>
        /// Saves the cloud anchor to room.
        /// </summary>
        /// <param name="room">The room to save the anchor to.</param>
        /// <param name="anchor">The Anchor to save.</param>
        public void SaveCloudAnchorToRoom(int room, XPAnchor anchor)
        {
            m_RoomAnchorsDict.Add(room, anchor);
        }

        /// <summary>
        /// Resolves a room request.
        /// </summary>
        /// <param name="netMsg">The resolve room request.</param>
        private void OnGetAnchorIdFromRoomRequest(NetworkMessage netMsg)
        {
            var roomMessage = netMsg.ReadMessage<AnchorIdFromRoomRequestMessage>();
            XPAnchor anchor;
            bool found = m_RoomAnchorsDict.TryGetValue(roomMessage.RoomId, out anchor);
            AnchorIdFromRoomResponseMessage response = new AnchorIdFromRoomResponseMessage
            {
                Found = found,
            };

            if (found)
            {
                response.AnchorId = anchor.CloudId;
            }

            NetworkServer.SendToClient(netMsg.conn.connectionId, RoomSharingMsgType.AnchorIdFromRoomResponse, response);
        }

        // custom message
        public void StartGame(float time)
        {
            // send time
            TimerMessage msg = new TimerMessage();
            msg.totalTime = time;
            NetworkServer.SendToAll(RoomSharingMsgType.MyResponse, msg);
            Debug.Log("start game from the server!: " + msg.totalTime);
        }
    }
}
