using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum BlockColors
{
    purple,
    white,
    pink,
    yellow
}

public class BlockBehavior : NetworkBehaviour
{
    public BlockColors blockColor;

    void Update()
    {
        // transform.position = Vector3.Lerp(transform.position, transform.position + new Vector3(0, 0.01f, 0), 0.1f);
    }
}
