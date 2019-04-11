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
        lobby, play, end, finale
    }

    public class GameManager : NetworkBehaviour
    {
        [Header("Managers")]
        [SerializeField] CloudAnchorController cloudAnchorController;

        [Header("GameObjects")]
        [SerializeField] GameObject wallPrefab;
        [SerializeField] GameObject[] blockPrefabs;
        [SerializeField] GameObject[] combinedBlockPrefabs;
        [SerializeField] int slotNum = 20;
        [SerializeField] int blockNums;
        [SerializeField] int combinedPairs;
        [SerializeField] GameObject helperPrefab;
        List<GameObject> blocks = new List<GameObject>();
        [SerializeField] Light directionalLight;

        [SerializeField] float wallHeight = 1f;
        float initTime;
        [SerializeField] float totalTime = 60f;
        string min;
        string sec;
        bool isTicking;

        Color progressColor;
        Color alertColor;
        [SerializeField] AudioClip beep;
        [SerializeField] AudioClip beepLast;
        AudioSource audioSource;

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
            progressColor = UIController.Instance.timerSliderFillColor.color;
            alertColor = Color.red;
            audioSource = GetComponent<AudioSource>();
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
                if (score >= slotNum + 1 || totalTime <= 0)
                {
                    EndGame();
                    return;
                }

                totalTime -= Time.deltaTime;
                min = Mathf.FloorToInt(totalTime / 60).ToString("00");
                sec = Mathf.FloorToInt(totalTime % 60).ToString("00");

                if (totalTime <= 0)
                {
                    UIController.Instance.timerSlider.value = 0;
                    UIController.Instance.timer.text = "00:00";
                    audioSource.PlayOneShot(beepLast, 0.8f);
                }
                else
                {
                    if (totalTime < 10 && !isTicking)
                    {
                        isTicking = true;
                        StartCoroutine(CountDown());
                    }

                    UIController.Instance.timerSlider.value = Mathf.Clamp01(totalTime / initTime);
                    UIController.Instance.timer.text = (min + ":" + sec);
                }
            }
        }

        #endregion

        IEnumerator CountDown()
        {
            float lerpFactor = 0;
            float duration = 0.5f;

            Color c1 = alertColor;
            Color c2 = progressColor;
            bool playSound = true;

            while (gamestate != GameStates.end && totalTime > 1)
            {
                lerpFactor += Time.deltaTime / duration;
                UIController.Instance.timerSliderFillColor.color = Color.Lerp(c1, c2, lerpFactor);

                if (lerpFactor > 1f)
                {
                    lerpFactor = 0;
                    Color temp = c1;
                    c1 = c2;
                    c2 = temp;

                    if (playSound)
                    {
                        audioSource.PlayOneShot(beep, 0.8f);
                    }

                    playSound = !playSound;
                }

                yield return null;
            }

            UIController.Instance.timerSliderFillColor.color = alertColor;
        }

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
                int index = i % blockPrefabs.Length;

                GameObject block = Instantiate(blockPrefabs[index], GetRandomPosFromPoint(wall.transform.position, spawnRadius, absHeight), Random.rotation);
                NetworkServer.Spawn(block);
                blocks.Add(block);
            }

            // spawn combined pair
            for (int i = 0; i < combinedPairs; i++)
            {
                GameObject cBlock1 = Instantiate(combinedBlockPrefabs[0], GetRandomPosFromPoint(wall.transform.position, spawnRadius, absHeight), Random.rotation);

                GameObject cBlock2 = Instantiate(combinedBlockPrefabs[1], GetRandomPosFromPoint(wall.transform.position, spawnRadius, absHeight), Random.rotation);

                NetworkServer.Spawn(cBlock1);
                NetworkServer.Spawn(cBlock2);

                blocks.Add(cBlock1);
                blocks.Add(cBlock2);
            }

            // spawn helpers
            StartCoroutine(SpawnHelpers());
        }

        IEnumerator SpawnHelpers()
        {
            GameObject helper1 = Instantiate(helperPrefab);
            NetworkServer.Spawn(helper1);

            yield return null;

            GameObject helper2 = Instantiate(helperPrefab);
            NetworkServer.Spawn(helper2);
        }

        Vector3 GetRandomPosFromPoint(Vector3 originPoint, float spawnRadius, float height)
        {
            var xz = Random.insideUnitCircle * spawnRadius;
            Vector2 originV2 = new Vector2(originPoint.x, originPoint.z);

            while (xz.magnitude < 0.5f)
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
            // Debug.Log("game over! total score is " + score);
            UIController.Instance.SetSnackbarText("game over! total score is " + score);
            StartCoroutine(TurnOffLight());
        }

        IEnumerator TurnOffLight()
        {
            // turn off the light for the finale
            Color currColor = directionalLight.color;
            Color targetColor = new Color(0, 0, 0);
            float lerpFactor = 0f;
            float duration = 3f;

            while (lerpFactor < 1f)
            {
                directionalLight.color = Color.Lerp(currColor, targetColor, lerpFactor);
                lerpFactor += Time.deltaTime / duration;
                yield return null;
            }

            directionalLight.enabled = false;
            gamestate = GameStates.finale;
            yield return LitBlocks();
        }

        IEnumerator LitBlocks()
        {
            List<BlockBehavior> matchedCombinedBlocks = new List<BlockBehavior>();

            // random order
            for (int i = 0; i < blocks.Count; i++)
            {
                BlockBehavior currBlock = blocks[i].GetComponent<BlockBehavior>();

                if (currBlock.blockState.value == BlockStates.matched)
                {
                    // skip the combined block for the last moment
                    if (currBlock.isCombinedBlock)
                    {
                        matchedCombinedBlocks.Add(currBlock);
                        continue;
                    }

                    if (isServer) currBlock.RpcFinaleParticles();
                    yield return new WaitForSeconds(0.2f);
                    if (isServer) currBlock.RpcFinale();
                    yield return new WaitForSeconds(0.5f);
                }
            }
            // combined block goes last!
            if (matchedCombinedBlocks != null)
            {
                // we only need one particle effect
                if (isServer) matchedCombinedBlocks[0].RpcFinaleParticles();
                yield return new WaitForSeconds(0.2f);

                for (int i = 0; i <= matchedCombinedBlocks.Count; i++)
                {
                    // this call affects both sides, but the server's combined block is different from
                    // the client block, so it does not matter to the server side
                    if (isServer) matchedCombinedBlocks[i].RpcFinale();
                    // this call does not affect the client side
                    // the server's combined block is affected by this call
                    if (isServer) matchedCombinedBlocks[i].OnFinale();
                }
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(1f);

            float timeSpent = initTime - totalTime;
            string min = Mathf.FloorToInt(timeSpent / 60).ToString("00");
            string sec = Mathf.RoundToInt(timeSpent % 60).ToString("00");
            // UIController.Instance.ShowEndUI(score.ToString(), min, sec);
        }

        public void AddScore()
        {
            if (!isServer) return;
            score++;
        }
    }
}
