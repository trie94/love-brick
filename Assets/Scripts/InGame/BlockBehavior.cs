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

    Renderer rend;
    [SerializeField] float blinkSpeed;
    [SerializeField] float shiverSpeed;

    AudioSource audioSource;
    [SerializeField] AudioClip hoverSound;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        rend.material.SetFloat("_MKGlowPower", 0f);
        rend.material.SetFloat("_MKGlowTexStrength", 1f);
    }

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
