namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
	using TMPro;

    public class GameController : MonoBehaviour
    {
		[SerializeField] float totalTime;
		string min;
		string sec;
		[SerializeField] TextMeshProUGUI timer;

        void Awake()
        {
			Debug.Log("hi from controller");
            EventManager.StartListening("OnGameStart", OnGameStart);
        }

        void OnGameStart(object data)
        {
            Debug.Log("Game start! and start count down");
			StartCoroutine(CountDown());
        }

        IEnumerator CountDown()
        {
			while (totalTime > 0f)
			{
				totalTime --;
				min = Mathf.FloorToInt(totalTime/60).ToString("00");
				sec = Mathf.RoundToInt(totalTime%60).ToString("00");
				// Debug.Log(min +":" +sec);
				timer.text = (min+":"+sec);
				yield return new WaitForSeconds(1f);

				if (totalTime <= 0f)
				{
					// final
					EventManager.TriggerEvent("OnGameEnd");
					timer.text = "00:00";
					yield break;
				}
			}
        }
    }

}