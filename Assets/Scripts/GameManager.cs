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
        [SerializeField] NetworkManager networkManager;
        [SerializeField] CloudAnchorController cloudAnchorController;

        [Header("GameObjects")]
        [SerializeField] GameObject lightPrefab;
        [SerializeField] GameObject block1;
        [SerializeField] GameObject wallPrefab;
        [SerializeField] float lightDistance = .3f;
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
            // GameObject wall = Instantiate(wallPrefab, cloudAnchor.transform.position + new Vector3(0, wallHeight, 0), cloudAnchor.transform.rotation);
            // GameObject light1 = Instantiate(lightPrefab, wall.transform.position + new Vector3(0, 0, lightDistance), Quaternion.identity);
            // GameObject light2 = Instantiate(lightPrefab, wall.transform.position + new Vector3(0, 0, -lightDistance), Quaternion.identity);
            // light1.transform.Rotate(0, 180, 0);

            // Debug.Log("spawn wall group");
            // NetworkServer.Spawn(wall);
            // NetworkServer.Spawn(light1);
            // NetworkServer.Spawn(light2);

            GameObject testBlock = Instantiate(block1, cloudAnchor.transform.position, Random.rotation);
            NetworkServer.Spawn(testBlock);
            Debug.Log("the anchor position: " + cloudAnchor.transform.position);
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