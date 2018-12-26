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
        [SerializeField] GameObject[] blocks;
        [SerializeField] int blockNums;
        [SerializeField] float wallHeight = 1f;
        [SerializeField] float totalTime = 60f;
        string min;
        string sec;

        [SyncVar] int score = 0;

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
        public bool isPlaying;

        public delegate void GameStartCallback();
        [SyncEvent] public event GameStartCallback EventOnGameStart;

        public delegate void ClientJoinCallback();
        public event ClientJoinCallback OnClientJoin;

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
            this.EventOnGameStart += OnGameStart;
        }

        void OnDisable()
        {
            cloudAnchorController.OnAnchorSaved -= OnAnchorSaved;
            this.EventOnGameStart -= OnGameStart;
        }

        #endregion

        void OnAnchorSaved(Transform anchor)
        {
            cloudAnchor = anchor;

            // spawn wall
            GameObject wall = Instantiate(wallPrefab, anchor.transform.position + new Vector3(0, wallHeight, 0), Quaternion.identity);

            var lookPos = Camera.main.transform.position - wall.transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            wall.transform.rotation = rotation;

            NetworkServer.Spawn(wall);

            // spawn blocks
            for (int i = 0; i < blockNums; i++)
            {
                int index = i % blocks.Length;
                float spawnRadius = 1.5f;
                float absHeight = 0.5f;

                GameObject block = Instantiate(blocks[index], GetRandomPosFromPoint(wall.transform.position, spawnRadius, absHeight), Random.rotation);
                NetworkServer.Spawn(block);
            }
        }

        Vector3 GetRandomPosFromPoint(Vector3 originPoint, float spawnRadius, float height)
        {
            var xz = Random.insideUnitCircle * spawnRadius;

            Vector2 originV2 = new Vector2(originPoint.x, originPoint.z);

            while (Mathf.Abs(Vector2.Distance(originV2, xz)) < 0.5f)
            {
                xz = Random.insideUnitCircle * spawnRadius;
            }
            var y = Random.Range(-height, height);
            Vector3 spawnPos = new Vector3(xz.x, y, xz.y) + originPoint;

            return spawnPos;
        }

        public void OnStartGame()
        {
            if (isServer && EventOnGameStart != null)
            {
                EventOnGameStart();
            }
        }

        IEnumerator CountDown()
        {
            while (totalTime > 0f)
            {
                totalTime--;
                min = Mathf.FloorToInt(totalTime / 60).ToString("00");
                sec = Mathf.RoundToInt(totalTime % 60).ToString("00");
                UIController.Instance.timer.text = (min + ":" + sec);
                yield return new WaitForSeconds(1f);
            }
            UIController.Instance.timer.text = "00:00";
            EndGame();
        }

        void OnGameStart()
        {
            Debug.Log("game has been started! start timer");
            UIController.Instance.SetSnackbarText("game has been started! start timer");
            StartCoroutine(CountDown());
            PlayerBehavior.LocalPlayer.GetColorBlocks();
            isPlaying = true;
        }

        void EndGame()
        {
            Debug.Log("game over!");
            isPlaying = false;
            UIController.Instance.ShowEndUI();
        }

        void AddScore()
        {
            score++;
        }
    }
}