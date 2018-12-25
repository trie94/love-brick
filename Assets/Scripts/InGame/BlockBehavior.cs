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

    public class BlockBehavior : NetworkBehaviour
    {
        public BlockColors blockColor;

        public enum BlockStates
        {
            idle, hovered, grabbed, released, matched
        }

        public BlockStates blockState = BlockStates.idle;

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
        SlotBehavior matchableSlot = null;

        void Awake()
        {
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
        }

        void Update()
        {
            if (blockState == BlockStates.released)
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, (transform.position.y - 0.2f), transform.position.z), 0.3f);
                transform.rotation = Quaternion.Lerp(transform.rotation, Random.rotation, 0.3f);
                return;
            }

            if (blockState == BlockStates.grabbed)
            {
                rend.material.SetFloat("_MKGlowPower", 0.1f);

                transform.position = Vector3.Lerp(transform.position, Camera.main.transform.position + Camera.main.transform.forward * 0.5f, 0.3f);

                transform.rotation = Quaternion.Lerp(transform.rotation, Camera.main.transform.rotation, 0.3f);

                GetMatchableSlot();
            }

            if (blockState == BlockStates.matched)
            {
                curGlow = Mathf.Lerp(curGlow, 0f, 0.3f);
                rend.material.SetFloat("_MKGlowPower", curGlow);

                transform.position = Vector3.Lerp(transform.position, matchableSlot.transform.position, 0.3f);

                transform.rotation = Quaternion.Lerp(transform.rotation, matchableSlot.transform.rotation, 0.3f);
            }

            if (blockState == BlockStates.hovered)
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

            if ((blockState == BlockStates.idle) && (rend.material.GetFloat("_MKGlowPower") != 0f))
            {
                rend.material.SetFloat("_MKGlowPower", 0f);
            }
        }

        public void OnHover()
        {
            blockState = BlockStates.hovered;
            // init
            curGlow = glowLerpFactor = 0f;
            maxGlow = 0.4f;
            minGlow = 0.2f;
            startPos = transform.position;
            targetPos = startPos + Random.insideUnitSphere * 0.01f;
        }

        public void OnGrab()
        {
            if (blockState == BlockStates.released) return;
            blockState = BlockStates.grabbed;
            audioSource.PlayOneShot(grabSound, 0.8f);
        }

        public void OnRelease()
        {
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
            blockState = BlockStates.idle;
        }

        public void OnMatch()
        {
            // future work -- animation to the wall
            blockState = BlockStates.matched;
            curGlow = rend.material.GetFloat("_MKGlowPower");
            audioSource.PlayOneShot(matchSound);

            if (col) col.enabled = false;
            if (matchableSlot) matchableSlot.slotState = SlotStates.matched;
            Debug.Log("match");
        }

        IEnumerator ReleaseToIdle()
        {
            blockState = BlockStates.released;
            yield return new WaitForSeconds(0.5f);
            blockState = BlockStates.idle;
        }

        void GetMatchableSlot()
        {
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
    }
}
