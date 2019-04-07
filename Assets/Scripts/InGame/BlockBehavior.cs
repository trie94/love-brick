namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public enum BlockColors
    {
        purple, white, pink, yellow, purpleYellow
    }

    public enum BlockStates
    {
        idle, hovered, grabbed, released, matched
    }

    public class BlockBehavior : NetworkClientSyncBehaviour
    {
        public BlockColors blockColor;


        ClientSyncVarBool isCombined = new ClientSyncVarBool(false);

        [System.Serializable]
        public class ClientSyncVarBlockStates : ClientSyncVar<BlockStates>
        {
            public ClientSyncVarBlockStates(BlockStates states, System.Action<BlockStates, BlockStates> onChange = null)
                : base(states, onChange)
            { }

            public override void Serialize(NetworkWriter writer, BlockStates state)
            {
                writer.Write((int)state);
            }

            public override void Deserialize(NetworkReader reader, out BlockStates state)
            {
                state = (BlockStates)reader.ReadInt32();
            }
        }

        public bool isCombinedBlock;
        Renderer childRenderer;
        [SerializeField] float combinableDistance;
        [SerializeField] float combineDistance;
        [SerializeField] GameObject combinePos;
        bool isDecombining;

        bool hasEnteredCombinableArea;

        BlockBehavior pairBlock = null;
        public static List<BlockBehavior> combinedBlockBehaviors = new List<BlockBehavior>();

        public ClientSyncVarBlockStates blockState = new ClientSyncVarBlockStates(BlockStates.idle);

        public bool isMatchable { get; set; }

        Renderer rend;
        [SerializeField] float glowLerpSpeed;
        [SerializeField] float shiverLerpSpeed;
        float glowLerpFactor;
        float shiverLerpFactor;

        float maxGlow;
        float minGlow;

        float maxTexGlow;
        float minTexGlow;

        float curGlow;
        float curTexGlow;

        [SerializeField] Texture2D glowTex;

        Vector3 startPos;
        Vector3 targetPos;
        Quaternion releaseRot;

        Collider col;

        AudioSource audioSource;
        [SerializeField] AudioClip grabSound;
        [SerializeField] AudioClip releaseSound;
        [SerializeField] AudioClip matchSound;
        [SerializeField] AudioClip combineSound;
        [SerializeField] AudioClip decombineSound;

        SlotHelper slotHelper;
        List<SlotBehavior> potentialSlots = new List<SlotBehavior>();

        [SerializeField] float matchableDistance;
        [SerializeField] float offset = 0.1f;
        SlotBehavior matchableSlot = null;

        public static TargetHelper[] targetHelpers;
        public GameObject particle;

        public override void Awake()
        {
            base.Awake();
            rend = GetComponent<Renderer>();
            audioSource = GetComponent<AudioSource>();
            col = GetComponent<Collider>();
            rend.material.SetFloat("_MKGlowPower", 0f);
            rend.material.SetFloat("_MKGlowTexStrength", 0f);
            startPos = transform.position;
            slotHelper = FindObjectOfType<SlotHelper>();
            for (int i = 0; i < slotHelper.slotBehaviors.Length; i++)
            {
                // combined blocks
                if (isCombinedBlock)
                {
                    if (slotHelper.slotBehaviors[i].slotColor == BlockColors.purpleYellow)
                    {
                        potentialSlots.Add(slotHelper.slotBehaviors[i]);
                    }
                }
                else    // regular block
                {
                    if (slotHelper.slotBehaviors[i].slotColor == blockColor)
                    {
                        potentialSlots.Add(slotHelper.slotBehaviors[i]);
                    }
                }

            }

            if (isCombinedBlock)
            {
                combinedBlockBehaviors.Add(this);

                // find the child renderer
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != rend)
                    {
                        childRenderer = renderers[i];
                    }
                }
                childRenderer.enabled = false;
            }
        }

        public override void Update()
        {
            base.Update();

            if (!hasAuthority) return;
            if (GameManager.Instance.gamestate != GameStates.play) return;

            if (blockState.value == BlockStates.released)
            {
                // when decombine
                if (isCombinedBlock && isDecombining)
                {
                    childRenderer.gameObject.transform.position = Vector3.Lerp(childRenderer.gameObject.transform.position, pairBlock.transform.position, 0.3f);
                    childRenderer.gameObject.transform.rotation = Quaternion.Lerp(childRenderer.gameObject.transform.rotation, releaseRot, 0.3f);
                }

                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, (transform.position.y - 0.03f), transform.position.z), 0.3f);
                transform.rotation = Quaternion.Lerp(transform.rotation, releaseRot, 0.3f);
            }

            else if (blockState.value == BlockStates.grabbed)
            {
                // combined-blocks?
                if (isCombinedBlock)
                {
                    if (!pairBlock) FindOtherCombinedBlock();

                    if (targetHelpers == null || targetHelpers.Length < 2)
                    {
                        targetHelpers = FindObjectsOfType<TargetHelper>();
                    }

                    // if the distance is close enough
                    float dist = Vector3.Distance(targetHelpers[0].transform.position, targetHelpers[1].transform.position);
                    if (isCombined.value)
                    {
                        if (pairBlock && pairBlock.blockState.value == BlockStates.matched)
                        {
                            OnMatch();
                            PlayerBehavior.LocalPlayer.playerState = PlayerBehavior.PlayerStates.match;
                            PlayerBehavior.LocalPlayer.CmdRequestToAddScore();
                            return;
                        }
                        else if (dist > combinableDistance + offset || pairBlock.blockState.value != BlockStates.grabbed)
                        {
                            OnDeCombine();
                        }
                        else
                        {
                            // combined, block follows the center position
                            Vector3 centerPos = (targetHelpers[0].transform.position + targetHelpers[1].transform.position) / 2;
                            transform.position = Vector3.Lerp(transform.position, centerPos, 0.3f);
                        }
                    }
                    else    // not combined
                    {
                        if (pairBlock)
                        {
                            if (pairBlock.blockState.value == BlockStates.grabbed)
                            {
                                if (dist > combinableDistance)
                                {
                                    pairBlock.GetComponent<Renderer>().enabled = true;
                                    childRenderer.enabled = false;
                                    hasEnteredCombinableArea = false;
                                }
                                // combinable area to fake lerping
                                else if (dist <= combinableDistance)
                                {
                                    if (!hasEnteredCombinableArea)
                                    {
                                        OnEnterCombine();
                                        hasEnteredCombinableArea = true;
                                    }
                                    // lerp the child object to the fake position
                                    childRenderer.gameObject.transform.position = Vector3.Lerp(childRenderer.gameObject.transform.position, combinePos.transform.position, 0.3f);
                                    childRenderer.gameObject.transform.rotation = Quaternion.Lerp(childRenderer.gameObject.transform.rotation, combinePos.transform.rotation, 0.3f);
                                }

                                if (dist <= combineDistance)
                                {
                                    OnCombine();
                                }
                            }
                            else
                            {
                                if (isDecombining)
                                {
                                    // decombining
                                    childRenderer.gameObject.transform.position = Vector3.Lerp(childRenderer.gameObject.transform.position, pairBlock.transform.position, 0.3f);
                                    childRenderer.gameObject.transform.rotation = Quaternion.Lerp(childRenderer.gameObject.transform.rotation, releaseRot, 0.3f);

                                    if (Vector3.Distance(childRenderer.gameObject.transform.position, pairBlock.transform.position) <= 0.2f)
                                    {
                                        OnEndDecombine();
                                    }
                                }
                            }
                        }
                    }
                }

                // both
                rend.material.SetFloat("_MKGlowPower", maxGlow * 0.5f);
                rend.material.SetFloat("_MKGlowTexStrength", maxTexGlow * 0.5f);

                if (isCombinedBlock && childRenderer.enabled)
                {
                    childRenderer.material.SetFloat("_MKGlowPower", maxGlow * 0.5f);
                    childRenderer.material.SetFloat("_MKGlowTexStrength", maxTexGlow * 0.5f);
                }

                if (!isCombined.value)
                {
                    transform.position = Vector3.Lerp(transform.position, Camera.main.transform.position + Camera.main.transform.forward * 0.5f, 0.3f);
                }
                transform.rotation = Quaternion.Lerp(transform.rotation, Camera.main.transform.rotation, 0.3f);
                FindMatchableSlot();
            }

            else if (blockState.value == BlockStates.matched)
            {
                curGlow = Mathf.Lerp(curGlow, 0f, 0.3f);
                curTexGlow = Mathf.Lerp(curTexGlow, 0f, 0.3f);
                rend.material.SetFloat("_MKGlowPower", curGlow);
                rend.material.SetFloat("_MKGlowTexStrength", 0f);

                Vector3 offset = Vector3.zero;

                if (isCombinedBlock)
                {
                    float offsetX = 0f;
                    if (blockColor == BlockColors.purple)
                    {
                        offsetX = 0.0215f;
                    }
                    if (blockColor == BlockColors.yellow)
                    {
                        offsetX = -0.025f;
                    }
                    offset = new Vector3(offsetX, 0, 0);
                }

                transform.position = Vector3.Lerp(transform.position, matchableSlot.transform.position + offset, 0.3f);
                transform.rotation = Quaternion.Lerp(transform.rotation, matchableSlot.transform.rotation, 0.3f);
            }

            else if (blockState.value == BlockStates.hovered)
            {
                // hover effect
                if (glowLerpFactor > 1f)
                {
                    glowLerpFactor = 0f;
                    float tempGlow = minGlow;
                    minGlow = maxGlow;
                    maxGlow = tempGlow;

                    float tempTexGlow = minTexGlow;
                    minTexGlow = maxTexGlow;
                    maxTexGlow = minTexGlow;
                }

                if (shiverLerpFactor > 1f)
                {
                    shiverLerpFactor = 0f;
                    Vector3 tempPos = startPos;
                    startPos = targetPos;
                    targetPos = tempPos;
                }

                glowLerpFactor += Time.deltaTime * glowLerpSpeed;
                shiverLerpFactor += Time.deltaTime * shiverLerpSpeed;

                curGlow = Mathf.Lerp(minGlow, maxGlow, glowLerpFactor);
                curTexGlow = Mathf.Lerp(minTexGlow, maxTexGlow, glowLerpFactor);

                rend.material.SetFloat("_MKGlowPower", curGlow);
                rend.material.SetFloat("_MKGlowTexStrength", curTexGlow);

                transform.position = Vector3.Lerp(startPos, targetPos, shiverLerpFactor);
            }

            else if ((blockState.value == BlockStates.idle) && (rend.material.GetFloat("_MKGlowPower") != 0f))
            {
                rend.material.SetFloat("_MKGlowPower", 0f);
                rend.material.SetFloat("_MKGlowTexStrength", 0f);
            }
        }

        public void OnHover()
        {
            blockState.value = BlockStates.hovered;
            // init
            curGlow = glowLerpFactor = 0f;
            maxGlow = 0.4f;

            if (blockColor == BlockColors.purple || blockColor == BlockColors.pink)
            {
                maxGlow = 1f;
            }
            minGlow = 0f;

            curTexGlow = 0f;
            maxTexGlow = 0.4f;

            if (blockColor == BlockColors.purple || blockColor == BlockColors.pink)
            {
                maxTexGlow = 1f;
            }
            minTexGlow = 0f;
            startPos = transform.position;
            targetPos = startPos + Random.insideUnitSphere * 0.01f;
        }

        public void OnGrab()
        {
            if (blockState.value == BlockStates.released) return;
            blockState.value = BlockStates.grabbed;
            audioSource.PlayOneShot(grabSound, 1f);
        }

        public void OnRelease()
        {
            if (isCombinedBlock && isCombined.value && pairBlock)
            {
                OnDeCombine();
            }

            if (matchableSlot)
            {
                matchableSlot.slotState = SlotStates.idle;
                matchableSlot = null;
            }
            audioSource.PlayOneShot(releaseSound, 1f);
            releaseRot = Random.rotation;
            StartCoroutine(ReleaseToIdle());
        }

        public void OnIdle()
        {
            blockState.value = BlockStates.idle;
        }

        public void OnMatch()
        {
            blockState.value = BlockStates.matched;
            isMatchable = false;
            curGlow = rend.material.GetFloat("_MKGlowPower");
            curTexGlow = rend.material.GetFloat("_MKGlowTexStrength");
            audioSource.PlayOneShot(matchSound);

            if (col) col.enabled = false;
            if (matchableSlot)
            {
                matchableSlot.slotState = SlotStates.idle;
                matchableSlot.isMatched = true;
                matchableSlot.matchedBlock = this;
            }
            Debug.Log("match");

            // combined block
            if (isCombinedBlock && pairBlock) pairBlock = null;
            UIController.Instance.SetSnackbarText("match, slot: " + matchableSlot + " / slot state: " + matchableSlot.slotState);
        }

        public void OnFinale()
        {
            Color c = rend.material.GetColor("_Color");
            float glowPower;
            float glowTexStrength;

            if (blockColor == BlockColors.white)
            {
                // c.a = 100 / 255f;
                glowPower = 0.1f;
                glowTexStrength = 0.3f;
            }
            else if (blockColor == BlockColors.purple)
            {
                // c.a = 115 / 255f;
                glowPower = 0.5f;
                glowTexStrength = 0.5f;
            }
            else if (blockColor == BlockColors.yellow)
            {
                // c.a = 100 / 255f;
                glowPower = 0.5f;
                glowTexStrength = 0.1f;
            }
            else    // pink
            {
                // c.a = 255 / 255f;
                glowPower = 0.5f;
                glowTexStrength = 0.5f;
            }

            rend.material.SetColor("_Color", c);
            rend.material.SetTexture("_MKGlowTex", glowTex);
            rend.material.SetFloat("_MKGlowPower", glowPower);
            rend.material.SetFloat("_MKGlowTexStrength", glowTexStrength);
            // add sound later on
        }

        void OnEnterCombine()
        {
            childRenderer.enabled = true;
            pairBlock.GetComponent<Renderer>().enabled = false;
            // move the child object to the pair block position
            childRenderer.gameObject.transform.position = pairBlock.transform.position;
            UIController.Instance.SetSnackbarText("on enter combine");
        }

        void OnCombine()
        {
            Debug.Log("combine");
            isCombined.value = true;
            hasEnteredCombinableArea = false;

            // stop lerping and fix the position
            childRenderer.gameObject.transform.position = combinePos.transform.position;
            childRenderer.gameObject.transform.rotation = combinePos.transform.rotation;
            audioSource.PlayOneShot(combineSound);
            UIController.Instance.SetSnackbarText("on combine");
        }

        void OnDeCombine()
        {
            isCombined.value = false;
            isMatchable = false;
            isDecombining = true;
            hasEnteredCombinableArea = false;
            Debug.Log("de-combine");
            audioSource.PlayOneShot(decombineSound);
            UIController.Instance.SetSnackbarText("de-combine, block state: " + blockState.value);
        }

        void OnEndDecombine()
        {
            pairBlock.GetComponent<Renderer>().enabled = true;
            childRenderer.enabled = false;
            isDecombining = false;
            UIController.Instance.SetSnackbarText("on end de-combine");
        }

        IEnumerator ReleaseToIdle()
        {
            if (isCombinedBlock && isDecombining)
            {
                OnEndDecombine();
            }

            blockState.value = BlockStates.released;
            yield return new WaitForSeconds(0.5f);
            blockState.value = BlockStates.idle;
        }

        void FindMatchableSlot()
        {
            if (isCombinedBlock && !isCombined.value)
            {
                if (matchableSlot != null)
                {
                    matchableSlot = null;
                }
                return;
            }

            float minDist = Mathf.Infinity;
            foreach (SlotBehavior s in potentialSlots)
            {
                float dist = Vector3.Distance(s.transform.position, transform.position);
                if (dist < minDist)
                {
                    if (matchableSlot != s && matchableSlot != null)
                    {
                        matchableSlot.slotState = SlotStates.idle;
                        isMatchable = false;
                        matchableSlot = null;
                    }

                    if (dist <= matchableDistance && !s.isMatched)
                    {
                        matchableSlot = s;
                        minDist = dist;
                    }
                }
            }

            if (matchableSlot != null)
            {
                matchableSlot.slotState = SlotStates.hover;
                isMatchable = true;
            }
        }

        void FindOtherCombinedBlock()
        {
            Debug.Log("there is no pair block, try to find one");
            UIController.Instance.SetSnackbarText("there is no pair block, try to find one");

            for (int i = 0; i < combinedBlockBehaviors.Count; i++)
            {
                BlockBehavior currentBlock = combinedBlockBehaviors[i];
                // skip self
                if (currentBlock == this) continue;

                if (currentBlock.blockState.value == BlockStates.grabbed)
                {
                    pairBlock = currentBlock;
                    UIController.Instance.SetSnackbarText("pair found: " + pairBlock);
                    return;
                }
            }
        }
    }
}
