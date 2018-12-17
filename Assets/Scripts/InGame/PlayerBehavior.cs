namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public class PlayerBehavior : NetworkBehaviour
    {
        static PlayerBehavior s_localPlayer;
        public static PlayerBehavior LocalPlayer
        {
            get { return s_localPlayer; }
        }

        NetworkIdentity identity { get; set; }
        int _playerIndex = -1;
        public int playerIndex
        {
            get { return _playerIndex; }
        }

        void Start()
        {
            identity = GetComponent<NetworkIdentity>();
            if (!isLocalPlayer) return;

            if (s_localPlayer != null)
            {
                Debug.LogError("local player already exists");
            }
            s_localPlayer = this;

            _playerIndex = FindObjectsOfType<PlayerBehavior>().Length - 1;
            Debug.Log("player index: " + playerIndex);
            AssignColor();
        }

        void OnDestroy()
        {
            if (this == s_localPlayer)
            {
                s_localPlayer = null;
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            Debug.Log("client start");
        }

        void AssignColor()
        {
            if (playerIndex == 0)
            {
                UIController.Instance.GetHostColor();
            }
            else
            {
                UIController.Instance.GetClientColor();
            }
        }

        void SpawnBlocks()
        {
            // spawn blocks with client authority
        }
    }

}