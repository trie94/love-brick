namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public enum BlockColors
    {
        purple, white, pink, yellow
    }

    public class BlockBehavior : NetworkClientSyncBehaviour
    {
        public BlockColors blockColor;

        public enum BlockStates
        {
            idle, hovered, grabbed, released, matched
        }

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
        [SerializeField] float combinableDistance;
        BlockBehavior pairBlock = null;
        public ClientSyncVarVector3 combinePos = new ClientSyncVarVector3(Vector3.zero);
        public static List<BlockBehavior> combinedBlockBehaviors = new List<BlockBehavior>();

        public ClientSyncVarBlockStates blockState = new ClientSyncVarBlockStates(BlockStates.idle);

        public bool isMatchable { get; set; }

        Renderer rend;
        [SerializeField] float glowLerpSpeed;
        [SerializeField] float shiverLerpSpeed;
        float glowLerpFactor;
        float shiverLerpFactor;

        float maxGlow = 0.4f;
        float minGlow = 0f;
        float curGlow;

        Vector3 startPos;
        Vector3 targetPos;

        Collider col;

        AudioSource audioSource;
        [SerializeField] AudioClip grabSound;
        [SerializeField] AudioClip releaseSound;
        [SerializeField] AudioClip matchSound;

        SlotHelper slotHelper;
        List<SlotBehavior> potentialSlots = new List<SlotBehavior>();

        [SerializeField] float matchableDistance;
        [SerializeField] float offset = 0.1f;
        SlotBehavior matchableSlot = null;

        public static TargetHelper[] targetHelpers;

        public override void Awake()
        {
            base.Awake();
            rend = GetComponent<Renderer>();
            audioSource = GetComponent<AudioSource>();
            col = GetComponent<Collider>();
            rend.material.SetFloat("_MKGlowPower", 0f);
            rend.material.SetFloat("_MKGlowTexStrength", 1f);
            startPos = transform.position;
            slotHelper = FindObjectOfType<SlotHelper>();
            for (int i = 0; i < slotHelper.slotBehaviors.Length; i++)
            {
                if (slotHelper.slotBehaviors[i].slotColor.ToString() == blockColor.ToString())
                {
                    potentialSlots.Add(slotHelper.slotBehaviors[i]);
                }
            }

            if (isCombinedBlock)
            {
                combinedBlockBehaviors.Add(this);
            }
        }

        public override void Update()
        {
            base.Update();

            if (!hasAuthority) return;

            if (blockState.value == BlockStates.released)
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, (transform.position.y - 0.2f), transform.position.z), 0.3f);
                transform.rotation = Quaternion.Lerp(transform.rotation, Random.rotation, 0.3f);
            }

            else if (blockState.value == BlockStates.grabbed)
            {
                // combined?
                if (isCombinedBlock)
                {
                    if (isCombined.value)
                    {
                        if (targetHelpers != null && targetHelpers.Length >= 2)
                        {
                            float dist = Vector3.Distance(targetHelpers[0].transform.position, targetHelpers[1].transform.position);
                            if ((dist > combinableDistance + offset) || pairBlock.blockState.value != BlockStates.grabbed)
                            {
                                OnDeCombine();
                            }
                            else
                            {
                                Vector3 centerPos = (targetHelpers[0].transform.position + targetHelpers[1].transform.position) / 2;

                                transform.position = Vector3.Lerp(transform.position, centerPos, 0.3f);
                                UIController.Instance.SetSnackbarText("combined and lerping to: " + centerPos);
                                Debug.Log(targetHelpers[0].transform.position + " + " + targetHelpers[1].transform.position + ": " + centerPos);
                            }
                        }
                        else
                        {
                            targetHelpers = FindObjectsOfType<TargetHelper>();
                        }
                    }
                    else    // look for a pair
                    {
                        FindOtherCombinedBlock();
                    }
                }

                if (!isCombined.value)
                {
                    transform.position = Vector3.Lerp(transform.position, Camera.main.transform.position + Camera.main.transform.forward * 0.5f, 0.3f);
                }

                // both
                rend.material.SetFloat("_MKGlowPower", 0.1f);
                transform.rotation = Quaternion.Lerp(transform.rotation, Camera.main.transform.rotation, 0.3f);
                FindMatchableSlot();
            }

            else if (blockState.value == BlockStates.matched)
            {
                curGlow = Mathf.Lerp(curGlow, 0f, 0.3f);
                rend.material.SetFloat("_MKGlowPower", curGlow);

                transform.position = Vector3.Lerp(transform.position, matchableSlot.transform.position, 0.3f);

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
                rend.material.SetFloat("_MKGlowPower", curGlow);

                transform.position = Vector3.Lerp(startPos, targetPos, shiverLerpFactor);
            }

            else if ((blockState.value == BlockStates.idle) && (rend.material.GetFloat("_MKGlowPower") != 0f))
            {
                rend.material.SetFloat("_MKGlowPower", 0f);
            }
        }

        public void OnHover()
        {
            blockState.value = BlockStates.hovered;
            // init
            curGlow = glowLerpFactor = 0f;
            maxGlow = 0.4f;
            minGlow = 0.2f;
            startPos = transform.position;
            targetPos = startPos + Random.insideUnitSphere * 0.01f;
        }

        public void OnGrab()
        {
            if (blockState.value == BlockStates.released) return;
            blockState.value = BlockStates.grabbed;
            audioSource.PlayOneShot(grabSound, 0.8f);
        }

        public void OnRelease()
        {
            if (isCombined.value)
            {
                isCombined.value = false;
            }

            if (matchableSlot)
            {
                matchableSlot.slotState = SlotStates.idle;
                matchableSlot = null;
            }
            audioSource.PlayOneShot(releaseSound, 0.3f);
            StartCoroutine(ReleaseToIdle());
        }

        public void OnIdle()
        {
            blockState.value = BlockStates.idle;
        }

        public void OnMatch()
        {
            blockState.value = BlockStates.matched;
            curGlow = rend.material.GetFloat("_MKGlowPower");
            audioSource.PlayOneShot(matchSound);

            if (col) col.enabled = false;
            if (matchableSlot) matchableSlot.slotState = SlotStates.matched;
            Debug.Log("match");

            // combined block
            if (isCombinedBlock && pairBlock) pairBlock = null;
        }

        void OnCombine()
        {
            Debug.Log("combine");
            isCombined.value = true;
            UIController.Instance.SetSnackbarText("on combine");
        }

        void OnDeCombine()
        {
            Debug.Log("de-combine");
            isCombined.value = false;
            UIController.Instance.SetSnackbarText("de-combine, block state: " + blockState.value);
        }

        IEnumerator ReleaseToIdle()
        {
            blockState.value = BlockStates.released;
            yield return new WaitForSeconds(0.5f);
            blockState.value = BlockStates.idle;
        }

        void FindMatchableSlot()
        {
            if (isCombinedBlock && !isCombined.value) return;

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

                    if (dist <= matchableDistance && s.slotState != SlotStates.matched)
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
            // if there is a pair block
            if (pairBlock)
            {
                if (pairBlock.blockState.value == BlockStates.grabbed)
                {
                    // if the distance is close enough and the block is not matched
                    float dist = Vector3.Distance(this.transform.position, pairBlock.transform.position);

                    Debug.Log("there is a pair " + pairBlock + ", distance between the block and the pair is " + dist);

                    UIController.Instance.SetSnackbarText("there is a pair " + pairBlock + ", distance between the block and the pair is " + dist);

                    if (dist <= combinableDistance)
                    {
                        // combine
                        OnCombine();
                    }
                }
            }
            else    // if there is no pair block
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
}
