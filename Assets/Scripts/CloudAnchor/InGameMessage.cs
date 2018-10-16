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

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(blockPos);
            Debug.Log("serializing: " + blockPos);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            blockPos = reader.ReadVector3();
            Debug.Log("deserializing: " + blockPos);
        }
    }

    public class Score : MessageBase
    {
        public float totalScore;
    }
}