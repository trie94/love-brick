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
        public float totalBlock;
    }

    public class Score : MessageBase
    {
        public float totalScore;
    }
}