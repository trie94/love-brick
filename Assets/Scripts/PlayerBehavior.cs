namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public enum playerState
    {
        grab,
        grabbable,
        release,
        match
    }

    public class PlayerBehavior : MonoBehaviour
    {
        [SerializeField] float hoverDis;
        playerState state = playerState.release;
        GameObject interactiveBlock;
        Color originalColor;

        private void Update()
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;
            Touch touch = Input.GetTouch(0);

            if (Physics.Raycast(ray, out hit, hoverDis))
            {
                if (hit.transform.tag == this.tag)
                {
                    Debug.Log("interactable");
                    state = playerState.grabbable;
                    interactiveBlock = hit.transform.gameObject;
                    originalColor = interactiveBlock.GetComponent<MeshRenderer>().material.color;
                    interactiveBlock.GetComponent<MeshRenderer>().material.color = Color.red;

                    if ((touch.phase == TouchPhase.Began) || (touch.phase == TouchPhase.Stationary) || (touch.phase == TouchPhase.Moved))
                    {
                        Debug.Log("gatcha");
                        state = playerState.grab;
                    }
                }
            }
            else if (interactiveBlock != null)
            {
                interactiveBlock.GetComponent<MeshRenderer>().material.color = originalColor;
            }
        }
        void DrawGizmos()
        {
            if (Camera.main)
            {
                Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 1);
            }
        }

        void OnDrawGizmos()
        {
            DrawGizmos();
        }

        void OnDrawGizmosSelected()
        {
            DrawGizmos();
        }
    }

}