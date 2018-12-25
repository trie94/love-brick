namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public enum SlotColors
    {
        purple, white, pink, yellow
    }

    public enum SlotStates
    {
        idle, hover, matched
    }

    public class SlotBehavior : MonoBehaviour
    {
        Renderer rend;
        [SerializeField] float glowLerpSpeed;
        float glowLerpFactor;

        float maxGlow = 0.4f;
        float minGlow = 0f;
        float curGlow;

        public SlotStates slotState = SlotStates.idle;
        public SlotColors slotColor;

        void Awake()
        {
            rend = GetComponent<Renderer>();
            rend.material.SetFloat("_MKGlowPower", 0f);
            rend.material.SetFloat("_MKGlowTexStrength", 1f);
        }

        void Update()
        {
            if (slotState == SlotStates.hover)
            {
                if (glowLerpFactor > 1f)
                {
                    glowLerpFactor = 0f;
                    float tempGlow = minGlow;
                    minGlow = maxGlow;
                    maxGlow = tempGlow;
                }

                glowLerpFactor += Time.deltaTime * glowLerpSpeed;
                curGlow = Mathf.Lerp(minGlow, maxGlow, glowLerpFactor);
                rend.material.SetFloat("_MKGlowPower", curGlow);
            }
            else if (rend.material.GetFloat("_MKGlowPower") != 0f)
            {
                rend.material.SetFloat("_MKGlowPower", 0f);
            }
        }
    }
}
