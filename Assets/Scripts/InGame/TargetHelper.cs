namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public class TargetHelper : NetworkBehaviour
    {
        public NetworkIdentity identity { get; set; }
        public int helperIndex { get; set; }

        void Start()
        {
            identity = GetComponent<NetworkIdentity>();
            helperIndex = FindObjectsOfType<TargetHelper>().Length - 1;
            Debug.Log("helper is spawned! index: " + helperIndex);
        }

        void Update()
        {
            if (GameManager.Instance.gamestate == GameStates.play)
            {
                transform.position = Vector3.Lerp(transform.position, Camera.main.transform.position + Camera.main.transform.forward * 0.5f, 0.3f);

                transform.rotation = Quaternion.Lerp(transform.rotation, Camera.main.transform.rotation, 0.3f);
            }
        }
    }
}
