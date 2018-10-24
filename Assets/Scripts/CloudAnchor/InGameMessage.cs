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

    public class BlockHover : MessageBase
    {
        public bool isHit;
        public GameObject block;

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(isHit);
            writer.Write(block);
            Debug.Log("writer: " + block.name + " is hit: " + isHit);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            isHit = reader.ReadBoolean();
            block = reader.ReadGameObject();
            Debug.Log(block.name + " is hit: " + isHit);
        }
    }

    public class Score : MessageBase
    {
        public float totalScore;
    }
}