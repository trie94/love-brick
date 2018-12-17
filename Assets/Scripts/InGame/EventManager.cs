using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour {
	
	public class Event: UnityEvent<object>{}
	Dictionary<string, Event> eventDictionary;
	static EventManager eventManager;
	 public static EventManager Instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;
                if (!eventManager)
                {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else
                {
                    eventManager.Init();
                }
            }

            return eventManager;
        }
    }

	void Init()
	{
		if(eventDictionary == null)
		{
			eventDictionary = new Dictionary<string, Event>();
		}
	}

    public static void StartListening(string eventName, UnityAction<object> listener)
    {
        Event thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new Event();
            thisEvent.AddListener(listener);
            Instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, UnityAction<object> listener)
    {
        if (eventManager == null) return;
        Event thisEvent = null;
        if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

	public static void TriggerEvent (string eventName, object data = null)
	{
		Event thisEvent = null;
		if (Instance.eventDictionary.TryGetValue (eventName, out thisEvent))
		{
			thisEvent.Invoke(data);
		}
	}
}
