namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;
    using GoogleARCore;

#if UNITY_EDITOR
    using Input = GoogleARCore.InstantPreviewInput;
#endif
    public class GameManager : NetworkBehaviour
    {
        [Header("Managers")]
        [SerializeField] CloudAnchorController cloudAnchorController;

        [Header("GameObjects")]
        [SerializeField] GameObject wallPrefab;
        [SerializeField] GameObject block1;
        [SerializeField] float wallHeight = 1f;

        static GameManager s_instance;
        public static GameManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = FindObjectOfType<GameManager>();
                }
                return s_instance;
            }
        }

        public Transform cloudAnchor;

        public enum GameMode
        {
            Lobby,
            Idle,
            Start,
            End
        }

        GameMode gameStatus = GameMode.Lobby;

        #region Unity methods

        void Awake()
        {
            if (s_instance != null)
            {
                Debug.LogError("GameManager already exists!");
            }
            s_instance = this;
        }

        void OnEnable()
        {
            cloudAnchorController.OnAnchorSaved += OnAnchorSaved;
        }

        void OnDisable()
        {
            cloudAnchorController.OnAnchorSaved -= OnAnchorSaved;
        }

        #endregion

        void OnAnchorSaved(Transform anchor)
        {
            // get the anchor and spawn objects(wall and the blocks)
            cloudAnchor = anchor;
            GameObject wall = Instantiate(wallPrefab, new Vector3(0, wallHeight, 0), Quaternion.identity);
            NetworkServer.Spawn(wall);
        }

        void OnStart()
        {
            // start timer
        }

        void OnEnd()
        {

        }

        // /// <summary>
        // /// Gets the platform-specific prefabs.
        // private GameObject _GetWallPrefab()
        // {
        //     return Application.platform != RuntimePlatform.IPhonePlayer ?
        //         WallPrefab : ARKitWallPrefab;
        // }
    }
}