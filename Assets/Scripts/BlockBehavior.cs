using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBehavior : MonoBehaviour
{
    public bool isHit { get; set; }
    MeshRenderer meshRenderer;
    Color originalColor;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalColor = meshRenderer.material.color;
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
}
