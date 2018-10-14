namespace Love.Core
{
    using UnityEngine;
    using UnityEngine.Networking;

    public class InGameMessage : MessageBase
    {
    }
    public class TimerMessage : MessageBase
    {
        public float totalTime;
    }
}