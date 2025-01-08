using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
	// Serializable and Public
	public enum EventName
	{
		/// <summary> �޲Σ���ʼ����Ϊ��ͣ״̬ </summary>
		LevelInit,

		/// <summary> �޲� </summary>
		Pause,

		/// <summary> �޲� </summary>
		Resume,

		/// <summary> ����int����ǰCombo�� </summary>
		UpdateCombo,

		/// <summary> ����double����ǰ���� </summary>
		UpdateScore,

		/// <summary> ����JudgeResult </summary>
		UpdateJudge,

		/// <summary> ����float����ǰ�����ٷֱ� </summary>
		UpdatePercentage,

		/// <summary> ����(JudgeResult, InputInfo) </summary>
		AddJudgeAndInputInfo,

		/// <summary> �޲� </summary>
		UpdateText,

		/// <summary> �޲� </summary>
		TurningPointUnselect,

		/// <summary> ����int��ѡ�е�ͼ�� </summary>
		ChangeTrackLayer,

		/// <summary> ����float��Ԥ��������С </summary>
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