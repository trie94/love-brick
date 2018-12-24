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

    BlockStates blockState = BlockStates.idle;

    void OnHover()
    {
        // blink
        // shiver
    }

    void OnGrab()
    {

    }

    void OnRelease()
    {

    }

    void OnMatch()
    {
        // animation to the wall
    }
}
