namespace Love.Core
{
    using System;
    using UnityEngine.Networking;

    /// <summary>
    /// Anchor identifier from room request message.
    /// </summary>
    public class AnchorIdFromRoomRequestMessage : MessageBase
    {
        /// <summary>
        /// The room identifier to get the Anchor id from.
        /// </summary>
        public Int32 RoomId;

        /// <summary>
        /// Serialize the message.
        /// </summary>
        /// <param name="writer">Writer to write the message to.</param>
        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(RoomId);
        }

        /// <summary>
        /// Deserialize the message.
        /// </summary>
        /// <param name="reader">Reader to read the message from.</param>
        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            RoomId = reader.ReadInt32();
        }
    }
}
