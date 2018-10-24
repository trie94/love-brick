namespace Love.Core
{
    using UnityEngine.Networking;

    /// <summary>
    /// Room Sharing Message Types.
    /// </summary>
    public struct RoomSharingMsgType
    {
        /// <summary>
        /// The Anchor id from room request message type.
        /// </summary>
        public const short AnchorIdFromRoomRequest = MsgType.Highest + 1;

        /// <summary>
        /// The Anchor id from room response message type.
        /// </summary>
        public const short AnchorIdFromRoomResponse = MsgType.Highest + 2;

        // custom
        public const short timer = MsgType.Highest + 3;
        public const short blockSpawner = MsgType.Highest + 4;
        public const short blockHover = MsgType.Highest + 5;
    }
}
