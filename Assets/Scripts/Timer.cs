using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Love.Core;
using TMPro;

public class Timer : MonoBehaviour
{
    public float totalTime = 60f;
    string min;
    string sec;

    [SerializeField] CloudAnchorUIController UIController;

    public void OnCountDown()
    {
        StartCoroutine(CountDown());
        Debug.Log("on count down");
    }

    void UpdateTime(float time)
    {
        totalTime = time;
        Debug.Log(totalTime);
    }

    IEnumerator CountDown()
    {
        while (totalTime > 0f)
        {
            totalTime--;
            min = Mathf.FloorToInt(totalTime / 60).ToString("00");
            sec = Mathf.RoundToInt(totalTime % 60).ToString("00");
            UpdateTime(totalTime);
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
