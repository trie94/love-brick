using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBehavior : MonoBehaviour
{
    public bool isHit { get; set; }
    bool isGlowing;

    MeshRenderer meshRenderer;

    Color originalColor;
    [SerializeField] float lerpFactor = 1f;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.SetFloat("_MKGlowTexStrength", 0f);
        meshRenderer.material.SetFloat("_MKGlowPower", 0f);
        originalColor = meshRenderer.material.color;
    }

    private void Update()
    {
        if (isHit && !isGlowing)
        {
            // hover effect
            StartCoroutine(Glow());
        }
        else if (meshRenderer.material.GetFloat("_MKGlowTexStrength") != 0f
        || meshRenderer.material.GetFloat("_MKGlowPower") != 0f)
        {
            meshRenderer.material.SetFloat("_MKGlowTexStrength", 0f);
            meshRenderer.material.SetFloat("_MKGlowPower", 0f);
        }
    }

    private IEnumerator Glow()
    {
        isGlowing = true;
        float lerpTime = 0f;
        float glowPower = 0f;
        float minGlowPower = 0f;
        float maxGlowPower = 1f;

        // this should be changed
        while (true)
        {
            lerpTime += lerpFactor * Time.deltaTime;
            glowPower = Mathf.Lerp(minGlowPower, maxGlowPower, lerpTime);
            meshRenderer.material.SetFloat("_MKGlowTexStrength", glowPower);
            meshRenderer.material.SetFloat("_MKGlowPower", glowPower);

            if (lerpTime >= 1f)
            {
                float temp = minGlowPower;
                minGlowPower = maxGlowPower;
                maxGlowPower = temp;

                lerpTime = 0f;
            }

            if (!isHit)
            {
                isGlowing = false;
                yield break;
            }

            yield return null;
        }
    }
}
