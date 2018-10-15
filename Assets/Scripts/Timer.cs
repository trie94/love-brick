namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;
    using TMPro;

    public class Timer : MonoBehaviour
    {
        [SerializeField] float totalTime = 120f;
        string min;
        string sec;

        [SerializeField] CloudAnchorUIController UIController;
        [SerializeField] RoomSharingServer RoomSharingServer;

        public void OnTimerStart()
        {
            if (this.gameObject.activeSelf)
            {
                TimerMessage msg = new TimerMessage();
                msg.totalTime = totalTime;
                NetworkServer.SendToAll(RoomSharingMsgType.timer, msg);
                Debug.Log("on timer start: " + msg.totalTime);
                UIController.ShowGameUI();
                StartCoroutine(CountDown());
            }
            else
            {
                Debug.Log("this device is not activated: " + this.gameObject);
            }
        }

        public void StartCountDown()
        {
            if (this.gameObject.activeSelf)
            {
                StartCoroutine(CountDown());
            }
        }

        IEnumerator CountDown()
        {
            while (totalTime > 0f)
            {
                totalTime--;
                min = Mathf.FloorToInt(totalTime / 60).ToString("00");
                sec = Mathf.RoundToInt(totalTime % 60).ToString("00");
                UIController.timer.text = (min + ":" + sec);
                yield return new WaitForSeconds(1f);

                if (totalTime <= 0f)
                {
                    // final
                    UIController.timer.text = "00:00";
                    Debug.Log("game end");
                    UIController.ShowEndUI();
                    yield break;
                }
            }
        }
    }
}