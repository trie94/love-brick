namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public enum SlotStates
    {
        idle, hover
    }

    public class SlotBehavior : NetworkBehaviour
    {
        Renderer rend;
        Renderer[] rends;
        [SerializeField] float glowLerpSpeed;
        float glowLerpFactor;

        float maxGlow = 0.4f;
        float minGlow = 0f;

        float maxTexGlow = 0.4f;
        float minTexGlow = 0f;

        float curGlow;
        float curTexGlow;

        public BlockColors slotColor;
        public SlotStates slotState;
        public bool isCombinedSlot;
        public bool isMatched;
        public BlockBehavior matchedBlock;

        void Awake()
        {
            if (isCombinedSlot)
            {
                rends = GetComponentsInChildren<Renderer>();
                for (int i = 0; i < rends.Length; i++)
                {
                    rends[i].material.SetFloat("_MKGlowPower", 0f);
                    rends[i].material.SetFloat("_MKGlowTexStrength", 0f);
                }
            }
            else
            {
                rend = GetComponent<Renderer>();
                rend.material.SetFloat("_MKGlowPower", 0f);
                rend.material.SetFloat("_MKGlowTexStrength", 0f);
            }

            // darker colors should have bigger value
            if (slotColor == BlockColors.purple || slotColor == BlockColors.pink)
            {
                maxGlow = 1f;
                maxTexGlow = 1f;
            }
        }

        void Update()
        {
            if (GameManager.Instance.gamestate != GameStates.play) return;

            if (slotState == SlotStates.hover)
            {
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

                glowLerpFactor += Time.deltaTime * glowLerpSpeed;
                curGlow = Mathf.Lerp(minGlow, maxGlow, glowLerpFactor);
                curTexGlow = Mathf.Lerp(curTexGlow, minTexGlow, glowLerpFactor);

                if (isCombinedSlot)
                {
                    for (int i = 0; i < rends.Length; i++)
                    {
                        rends[i].material.SetFloat("_MKGlowPower", curGlow);

                        rends[i].material.SetFloat("_MKGlowTexStrength", curTexGlow);
                    }
                }
                else
                {
                    rend.material.SetFloat("_MKGlowPower", curGlow);

                    rend.material.SetFloat("_MKGlowTexStrength", curTexGlow);
                }
            }
            else
            {
                if (isCombinedSlot)
                {
                    for (int i = 0; i < rends.Length; i++)
                    {
                        if (rends[i].material.GetFloat("_MKGlowPower") != 0f)
                        {
                            rends[i].material.SetFloat("_MKGlowPower", 0f);
                            rends[i].material.SetFloat("_MKGlowTexStrength", 0f);
                        }

                    }
                }
                else
                {
                    if (rend.material.GetFloat("_MKGlowPower") != 0f)
                    {
                        rend.material.SetFloat("_MKGlowPower", 0f);
                        rend.material.SetFloat("_MKGlowTexStrength", 0f);
                    }
                }
            }
        }
    }
}
