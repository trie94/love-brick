namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

#if UNITY_EDITOR
    using Input = GoogleARCore.InstantPreviewInput;
#endif

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

        private void Update()
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;
            // Touch touch = Input.GetTouch(0);

            if (Physics.Raycast(ray, out hit, hoverDis))
            {
                if (hit.collider.tag == this.tag)
                {
                    state = playerState.grabbable;
                    interactiveBlock = hit.collider.gameObject;
                    interactiveBlock.GetComponent<BlockBehavior>().isHit = true;

                    // if ((touch.phase == TouchPhase.Began) || (touch.phase == TouchPhase.Stationary) || (touch.phase == TouchPhase.Moved))
                    // {
                    //     Debug.Log("gatcha");
                    //     state = playerState.grab;
                    // }
                }
            }
            else if (interactiveBlock != null)
            {
                interactiveBlock.GetComponent<BlockBehavior>().isHit = false;
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