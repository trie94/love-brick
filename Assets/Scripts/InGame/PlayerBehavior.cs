namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

#if UNITY_EDITOR
    using Input = GoogleARCore.InstantPreviewInput;
#endif
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

        public enum PlayerStates
        {
            idle, hover, grab, release, match
        }

        PlayerStates playerState = PlayerStates.idle;
        BlockBehavior currentBlock;
        BlockBehavior currentBlockBehavior;

        [SerializeField] float hoverDistance = 0.5f;

        bool isTouching;

        #region Unity methods

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

            if (playerIndex != 0)
            {
                UIController.Instance.GetClientColor();
            }
            else
            {
                UIController.Instance.GetHostColor();
            }
        }

        void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            if (GameManager.Instance.gamestate == GameStates.play)
            {
                // now able to play
                Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                RaycastHit hit;

                if (playerState == PlayerStates.match)
                {
                    playerState = PlayerStates.idle;
                }

                // release
                if (playerState == PlayerStates.grab && !isTouching)
                {
                    // if the slot is close enough, match the block with the slot
                    if (currentBlock.isMatchable)
                    {
                        playerState = PlayerStates.match;
                        currentBlock.OnMatch();
                        UIController.Instance.SetSnackbarText("match! " + currentBlock.gameObject);
                        Debug.Log("match");
                        CmdRequestToAddScore();
                        return;
                    }

                    playerState = PlayerStates.release;
                    UIController.Instance.SetSnackbarText("release! " + currentBlock.gameObject);
                    currentBlock.OnRelease();
                    currentBlock = null;
                    Debug.Log("release");
                    return;
                }

                // when hover, if you touch, then grab
                if (playerState == PlayerStates.hover && isTouching)
                {
                    playerState = PlayerStates.grab;
                    currentBlock.OnGrab();
                    UIController.Instance.SetSnackbarText("grab! " + currentBlock.gameObject);
                    Debug.Log("grab");
                    return;
                }

                // touching?
                if (Input.touchCount > 0)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled)
                    {
                        isTouching = false;
                    }
                    else
                    {
                        isTouching = true;
                    }
                }
                else
                {
                    isTouching = false;
                }

                if (Physics.Raycast(ray, out hit, hoverDistance))
                {
                    // check if the block is interactable
                    GameObject temp = hit.collider.gameObject;
                    if (playerState == PlayerStates.grab || playerState == PlayerStates.release) return;

                    if ((currentBlock == null || currentBlock.gameObject != temp) && temp.GetComponent<NetworkIdentity>().hasAuthority)
                    {
                        currentBlock = temp.GetComponent<BlockBehavior>();
                        playerState = PlayerStates.hover;
                        currentBlock.OnHover();
                        UIController.Instance.SetSnackbarText("hovering! " + currentBlock.name);
                        Debug.Log("hover");
                    }
                }
                else if (playerState != PlayerStates.idle && playerState != PlayerStates.grab)   // not hitting anything
                {
                    if (currentBlock)
                    {
                        currentBlock.OnIdle();
                    }
                    currentBlock = null;
                    playerState = PlayerStates.idle;
                    UIController.Instance.SetSnackbarText("idling! not hovering anything or non-interactable block!");
                    Debug.Log("idle");
                }

                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward);
            }
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

        #endregion

        public void GetColorBlocks()
        {
            BlockBehavior[] blocks = FindObjectsOfType<BlockBehavior>();
            Debug.Log(blocks.Length);

            for (int i = 0; i < blocks.Length; i++)
            {
                if (playerIndex == 0)
                {
                    if (blocks[i].blockColor == BlockColors.purple || blocks[i].blockColor == BlockColors.white)
                    {
                        blocks[i].GetComponent<NetworkIdentity>().AssignClientAuthority(this.connectionToClient);
                    }
                }
                else
                {
                    if (blocks[i].blockColor == BlockColors.pink || blocks[i].blockColor == BlockColors.yellow)
                    {
                        CmdSetAuthority(blocks[i].GetComponent<NetworkIdentity>(), identity);

                        // blocks[i].GetComponent<NetworkIdentity>().AssignClientAuthority(this.connectionToClient);
                    }
                    // Debug.Log("client assign authority!!");
                }
            }
        }

        [Command]
        void CmdSetAuthority(NetworkIdentity objectID, NetworkIdentity playerID)
        {
            objectID.AssignClientAuthority(playerID.connectionToClient);
        }

        [Command]
        void CmdRemoveAuthority(NetworkIdentity objectID, NetworkIdentity playerID)
        {
            objectID.RemoveClientAuthority(playerID.connectionToClient);
        }

        [Command]
        void CmdRequestToAddScore()
        {
            Debug.Log("client requests to add score");
            GameManager.Instance.AddScore();
            UIController.Instance.SetSnackbarText("add score. current score is: " + GameManager.Instance.score);
        }
    }
}
