using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBehavior : MonoBehaviour
{
    public bool isHit { get; set; }
    MeshRenderer meshRenderer;
	Renderer renderer;
    Color originalColor;
    [SerializeField] float lerpFactor = 1f;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
		renderer = GetComponent<Renderer>();
        originalColor = meshRenderer.material.color;
		StartCoroutine(Glow());
    }

    private void Update()
    {
        if (isHit)
        {
            // hover effect
            meshRenderer.material.color = Color.red;
            Debug.Log(this.gameObject.name + "is hit");
        }
        else if (meshRenderer.material.color != originalColor)
        {
            // static
            meshRenderer.material.color = originalColor;
            Debug.Log(this.gameObject.name + "is no more hit");
        }
    }

    private IEnumerator Glow()
    {
		float lerpTime = 0f;
        float glowPower = 0f;
		float minGlowPower = 0f;
		float maxGlowPower = 1f;

		// float glowTexPower = 0f;
		// float minGlowTexPower = 0f;
		// float maxGlowTexPower = 5f;

		// this should be changed
        while (true)
        {
			lerpTime += lerpFactor * Time.deltaTime;
            glowPower = Mathf.Lerp(minGlowPower, maxGlowPower, lerpTime);
            meshRenderer.material.SetFloat("_MKGlowPower", glowPower);
            // meshRenderer.material.SetFloat("_MKGlowTexStrength", glowTexPower);

            if (lerpTime >= 1f)
            {
				float temp = minGlowPower;
				minGlowPower = maxGlowPower;
				maxGlowPower = temp;

				// float temp2 = minGlowTexPower;
				// minGlowPower = maxGlowTexPower;
				// maxGlowTexPower = temp2;
				lerpTime = 0f;
            }

            yield return null;
        }
    }
}
