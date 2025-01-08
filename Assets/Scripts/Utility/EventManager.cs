using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
	// Serializable and Public
	public enum EventName
	{
		/// <summary> 无参：初始化后为暂停状态 </summary>
		LevelInit,

		/// <summary> 无参 </summary>
		Pause,

		/// <summary> 无参 </summary>
		Resume,

		/// <summary> 传入int：当前Combo数 </summary>
		UpdateCombo,

		/// <summary> 传入double：当前分数 </summary>
		UpdateScore,

		/// <summary> 传入JudgeResult </summary>
		UpdateJudge,

		/// <summary> 传入float：当前分数百分比 </summary>
		UpdatePercentage,

		/// <summary> 传入(JudgeResult, InputInfo) </summary>
		AddJudgeAndInputInfo,

		/// <summary> 无参 </summary>
		UpdateText,

		/// <summary> 无参 </summary>
		TurningPointUnselect,

		/// <summary> 传入int：选中的图层 </summary>
		ChangeTrackLayer,

		/// <summary> 传入float：预设音量大小 </summary>
		PlayHitSound,
	};

	// Private
	Dictionary<EventName, UnityEvent> events;
	Dictionary<EventName, UnityEvent<object>> paramEvents;
	static EventManager eventManager;

	// Static

	// Defined Funtion
	private static EventManager Instance
	{
		get
		{
			if (!eventManager)
			{
				eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;
				if (!eventManager)
					Debug.LogError(
						"There needs to be one and only one active EventManager script on a GameObject in the scene.");
				else
					eventManager.Init();
			}

			return eventManager;
		}
	}

	void Init()
	{
		events ??= new();
		paramEvents ??= new();
	}

	public static void AddListener(EventName evtName, UnityAction listener)
	{
		if (Instance.events.TryGetValue(evtName, out UnityEvent evt))
		{
			evt.AddListener(listener);
		}
		else
		{
			evt = new UnityEvent();
			evt.AddListener(listener);
			Instance.events.Add(evtName, evt);
		}
	}

	public static void AddListener(EventName evtName, UnityAction<object> listener)
	{
		if (Instance.paramEvents.TryGetValue(evtName, out UnityEvent<object> evt))
		{
			evt.AddListener(listener);
		}
		else
		{
			evt = new();
			evt.AddListener(listener);
			Instance.paramEvents.Add(evtName, evt);
		}
	}

	public static void RemoveListener(EventName evtName, UnityAction listener)
	{
		if (eventManager == null) return;
		if (Instance.events.TryGetValue(evtName, out UnityEvent evt))
		{
			evt.RemoveListener(listener);
		}
		else
		{
			Debug.LogWarning("Invalid removing listener.");
		}
	}

	public static void RemoveListener(EventName evtName, UnityAction<object> listener)
	{
		if (eventManager == null) return;
		if (Instance.paramEvents.TryGetValue(evtName, out UnityEvent<object> evt))
		{
			evt.RemoveListener(listener);
		}
		else
		{
			Debug.LogWarning("Invalid removing listener.");
		}
	}

	public static void Trigger(EventName evtName)
	{
		if (Instance.events.TryGetValue(evtName, out UnityEvent evt))
		{
			evt.Invoke();
		}
		else
		{
			Debug.LogWarning("Invalid trigger event.");
		}
	}

	public static void Trigger(EventName evtName, object data)
	{
		if (Instance.paramEvents.TryGetValue(evtName, out UnityEvent<object> evt))
		{
			evt.Invoke(data);
		}
		else
		{
			Debug.LogWarning("Invalid event triggering.");
		}
	}

	// System Funtion
}