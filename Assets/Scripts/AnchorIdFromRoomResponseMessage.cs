namespace Love.Core
{
    using UnityEngine.Networking;

    /// <summary>
    /// Anchor identifier from room response message.
    /// </summary>
    public class AnchorIdFromRoomResponseMessage : MessageBase
    {
        /// <summary>
        /// True if the anchor id was found.
        /// </summary>
        public bool Found;

        /// <summary>
        /// The anchor id. Null if not found.
        /// </summary>
        public string AnchorId;

        /// <summary>
        /// Serialize the message.
        /// </summary>
        /// <param name="writer">Writer to write the message to.</param>
        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Found);
            writer.Write(AnchorId);
        }

        /// <summary>
        /// Deserialize the message.
        /// </summary>
        /// <param name="reader">Reader to read the message from.</param>
        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            Found = reader.ReadBoolean();
            AnchorId = reader.ReadString();
        }
    }
}
