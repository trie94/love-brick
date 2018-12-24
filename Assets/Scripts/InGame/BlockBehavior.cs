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
    [SerializeField] float blinkSpeed;
    [SerializeField] float shiverSpeed;
    Collider collider;

    AudioSource audioSource;
    [SerializeField] AudioClip hoverSound;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        collider = GetComponent<Collider>();
        rend.material.SetFloat("_MKGlowPower", 0f);
        rend.material.SetFloat("_MKGlowTexStrength", 1f);
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
            // get distance
            // if close enough
            // isMatchable = true;
            // else false
            transform.position = Vector3.Lerp(transform.position, Camera.main.transform.position + Camera.main.transform.forward * 0.7f, 0.3f);

            transform.rotation = Quaternion.Lerp(transform.rotation, Camera.main.transform.rotation, 0.3f);
        }

        if (blockState == BlockStates.idle)
        {

        }
    }

    public void OnHover()
    {
        blockState = BlockStates.hovered;
    }

    public void OnGrab()
    {
        blockState = BlockStates.grabbed;
    }

    public void OnRelease()
    {
        StartCoroutine(ReleaseToIdle());
    }

    public void OnMatch()
    {
        // animation to the wall
        if (collider)
        {
            collider.enabled = false;
        }
        Debug.Log("match");
    }

    IEnumerator ReleaseToIdle()
    {
        blockState = BlockStates.released;
        yield return new WaitForSeconds(0.5f);
        blockState = BlockStates.idle;
    }

    void Blink()
    {

    }
}
