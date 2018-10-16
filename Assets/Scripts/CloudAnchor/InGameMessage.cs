namespace Love.Core
{
    using UnityEngine;
    using UnityEngine.Networking;

    public class TimerMessage : MessageBase
    {
        public float totalTime;
    }

    public class BlockSpawner : MessageBase
    {
        public Vector3 blockPos;
        public Quaternion blockRot;

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(blockPos);
            writer.Write(blockRot);
            // Debug.Log("serializing: " + blockPos + ", " + blockRot);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            blockPos = reader.ReadVector3();
            blockRot = reader.ReadQuaternion();
            // Debug.Log("deserializing: " + blockPos + ", " + blockRot);
        }
    }

    public class Score : MessageBase
    {
        public float totalScore;
    }
}