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

    public enum GameStates
    {
        lobby, play, end
    }

    public class GameManager : NetworkBehaviour
    {
        [Header("Managers")]
        [SerializeField] CloudAnchorController cloudAnchorController;

        [Header("GameObjects")]
        [SerializeField] GameObject wallPrefab;
        [SerializeField] GameObject[] blocks;
        [SerializeField] GameObject[] combinedBlocks;
        [SerializeField] int slotNum = 20;
        [SerializeField] int blockNums;
        [SerializeField] int combinedPairs;
        [SerializeField] float wallHeight = 1f;
        float initTime;
        [SerializeField] float totalTime = 60f;
        string min;
        string sec;

        [SyncVar] public int score = 0;
        public GameStates gamestate = GameStates.lobby;

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
            initTime = totalTime;
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

        void Update()
        {
            if (gamestate == GameStates.play)
            {
                if (score >= slotNum || totalTime <= 0)
                {
                    EndGame();
                    return;
                }

                totalTime -= Time.deltaTime;
                min = Mathf.FloorToInt(totalTime / 60).ToString("00");
                sec = Mathf.FloorToInt(totalTime % 60).ToString("00");

                if (totalTime <= 0)
                {
                    UIController.Instance.timer.text = "00:00";
                }
                else
                {
                    UIController.Instance.timer.text = (min + ":" + sec);
                }
            }
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

            float spawnRadius = 1.5f;
            float absHeight = 0.5f;

            // spawn blocks
            for (int i = 0; i < blockNums; i++)
            {
                int index = i % blocks.Length;

                GameObject block = Instantiate(blocks[index], GetRandomPosFromPoint(wall.transform.position, spawnRadius, absHeight), Random.rotation);
                NetworkServer.Spawn(block);
            }

            // spawn combined pair
            for (int i = 0; i < combinedPairs; i++)
            {
                GameObject cBlock1 = Instantiate(combinedBlocks[0], GetRandomPosFromPoint(wall.transform.position, spawnRadius, absHeight), Random.rotation);

                GameObject cBlock2 = Instantiate(combinedBlocks[1], GetRandomPosFromPoint(wall.transform.position, spawnRadius, absHeight), Random.rotation);

                NetworkServer.Spawn(cBlock1);
                NetworkServer.Spawn(cBlock2);
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

        void OnGameStart()
        {
            gamestate = GameStates.play;
            UIController.Instance.SetSnackbarText("game has been started! start timer");
            PlayerBehavior.LocalPlayer.GetColorBlocks();
        }

        void EndGame()
        {
            gamestate = GameStates.end;
            Debug.Log("game over! total score is " + score);
            // UIController.Instance.SetSnackbarText("game over! total score is " + score);

            float timeSpent = initTime - totalTime;
            string min = Mathf.FloorToInt(timeSpent / 60).ToString("00");
            string sec = Mathf.RoundToInt(timeSpent % 60).ToString("00");

            UIController.Instance.ShowEndUI(score.ToString(), min, sec);
        }

        public void AddScore()
        {
            if (!isServer) return;
            score++;
        }
    }
}
