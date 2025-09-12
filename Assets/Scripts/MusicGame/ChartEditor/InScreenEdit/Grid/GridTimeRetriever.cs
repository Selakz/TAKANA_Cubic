using System;
using System.Linq;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class GridTimeRetriever : MonoBehaviour, ITimeRetriever
	{
		// Serializable and Public
		[SerializeField] private Toggle toggle;
		[SerializeField] private TMP_InputField gridDivisionInputField;
		[SerializeField] private Transform timeGridRoot;

		public event Action OnBeforeResetGrid = delegate { };
		public BpmList BpmList { get; private set; }

		public int GridDivision
		{
			get => gridDivision;
			set
			{
				value = Mathf.Max(1, value);
				gridDivisionInputField.text = value.ToString();
				if (LevelManager.Instance.LevelInfo.Preference is EditorPreference preference)
				{
					preference.TimeGridLineCount = value;
				}

				if (gridDivision != value)
				{
					gridDivision = value;
					ResetGrid();
				}
			}
		}

		// Private
		private T3Time upHeightTimeIncrement;
		private T3Time currentGridTime;
		private int gridDivision = 1;

		private readonly ObjectPool<TimeGridController> timeGridPool = new(
			() => Instantiate<GameObject>(lazyPrefab).GetComponent<TimeGridController>(),
			grid => grid.gameObject.SetActive(true),
			grid => grid.gameObject.SetActive(false),
			grid => Destroy(grid.gameObject));

		// Static
		private static LazyPrefab lazyPrefab;

		// Defined Functions
		public T3Time GetTimeStart(Vector3 position)
		{
			T3Time time = ITimeRetriever.GetCorrespondingTime(position.y);
			var ceilTime = BpmList.GetCeilTime(time, GridDivision, out _);
			var floorTime = BpmList.GetFloorTime(time, GridDivision, out _);
			var ceilDistance = Mathf.Abs(ceilTime - time);
			var floorDistance = Mathf.Abs(floorTime - time);
			if (ceilDistance > ISingletonSetting<InScreenEditSetting>.Instance.TimeSnapDistance &&
			    floorDistance > ISingletonSetting<InScreenEditSetting>.Instance.TimeSnapDistance)
				return time;
			return ceilDistance > floorDistance ? floorTime : ceilTime;
		}

		public T3Time GetHoldTimeEnd(Vector3 position)
		{
			T3Time time = GetTimeStart(position);
			var ceilTime = BpmList.GetCeilTime(time, GridDivision, out _);
			return time == ceilTime ? BpmList.GetCeilTime(ceilTime + 1, GridDivision, out _) : ceilTime;
		}

		public T3Time GetTrackTimeEnd(Vector3 position)
		{
			return ISingletonSetting<InScreenEditSetting>.Instance.IsInitialTrackLengthToEnd
				? LevelManager.Instance.Music.AudioLength
				: BpmList.GetCeilTime(
					ITimeRetriever.GetCorrespondingTime(position.y) +
					ISingletonSetting<InScreenEditSetting>.Instance.InitialTrackLength,
					GridDivision, out _);
		}

		public T3Time GetCeilTime(T3Time time) => BpmList.GetCeilTime(time, GridDivision, out _);

		public T3Time GetFloorTime(T3Time time) => BpmList.GetFloorTime(time, GridDivision, out _);

		public void ReleaseTimeGrid(TimeGridController timeGrid)
		{
			timeGridPool.Release(timeGrid);
		}

		public void ResetGrid()
		{
			if (BpmList is not null)
			{
				OnBeforeResetGrid.Invoke();
				currentGridTime = BpmList.GetFloorTime(LevelManager.Instance.Music.ChartTime, gridDivision, out _);
			}
		}

		private Color GetColor(int gridIndex)
		{
			int remainder = gridIndex % GridDivision;
			if (remainder == 0) return ISingletonSetting<InScreenEditSetting>.Instance.BeatColor;
			if (GridDivision % 3 == 0)
			{
				int[] target = { 1, 2, 4, 5 };
				if (target.Any(i => remainder == GridDivision * i / 6))
				{
					return ISingletonSetting<InScreenEditSetting>.Instance.TripletColor;
				}
			}

			if (GridDivision % 2 == 0)
			{
				if (remainder == GridDivision / 2)
					return ISingletonSetting<InScreenEditSetting>.Instance.QuaverColor;
				if (remainder == GridDivision / 4 || remainder == GridDivision * 3 / 4)
					return ISingletonSetting<InScreenEditSetting>.Instance.SemiQuaverColor;
			}

			return ISingletonSetting<InScreenEditSetting>.Instance.DefaultColor;
		}

		// Event Handlers
		private void BpmListOnUpdate()
		{
			EventManager.Instance.Invoke("Edit_OnBpmListUpdate");
			ResetGrid();
			if (LevelManager.Instance.LevelInfo.Preference is EditorPreference preference)
			{
				preference.BpmList = BpmList.ToDictionary();
			}
		}

		private void LevelOnLoad(LevelInfo levelInfo)
		{
			if (BpmList is not null) BpmList.OnBpmListUpdate -= BpmListOnUpdate;
			BpmList = new();
			if (levelInfo.Preference is EditorPreference preference)
			{
				GridDivision = preference.TimeGridLineCount;
				foreach (var pair in preference.BpmList)
				{
					BpmList.Add(pair.Key, pair.Value);
				}
			}

			if (BpmList.Count == 0) BpmList.Add(0, 100f);
			BpmList.OnBpmListUpdate += BpmListOnUpdate;
			EventManager.Instance.Invoke("Edit_OnBpmListUpdate");
			ResetGrid();
		}

		private void LevelOnReset(T3Time chartTime)
		{
			ResetGrid();
		}

		private void LevelOnSpeedUpdate(float speedRate)
		{
			upHeightTimeIncrement = ISingletonSetting<PlayfieldSetting>.Instance.UpperThreshold / speedRate;
			ResetGrid();
		}

		private void OnToggleChanged(bool isOn)
		{
			if (isOn)
			{
				timeGridRoot.gameObject.SetActive(true);
				InScreenEditManager.Instance.TimeRetriever = this;
			}
			else
			{
				timeGridRoot.gameObject.SetActive(false);
				InScreenEditManager.Instance.TimeRetriever = InScreenEditManager.FallbackTimeRetriever;
			}
		}

		private void OnGridDivisionInputFieldEndEdit(string content)
		{
			if (int.TryParse(content, out int division))
			{
				GridDivision = division;
			}

			gridDivisionInputField.SetTextWithoutNotify(GridDivision.ToString());
		}

		// System Functions
		void Awake()
		{
			toggle.onValueChanged.AddListener(OnToggleChanged);
			gridDivisionInputField.onEndEdit.AddListener(OnGridDivisionInputFieldEndEdit);
			lazyPrefab ??= new("Prefabs/EditorUI/InScreenEdit/TimeGrid", "TimeGridPrefab_OnLoad");
		}

		void Update()
		{
			if (timeGridRoot.gameObject.activeSelf && BpmList is not null)
			{
				while (currentGridTime < LevelManager.Instance.Music.ChartTime + upHeightTimeIncrement)
				{
					currentGridTime = BpmList.GetCeilTime(currentGridTime, GridDivision, out var gridIndex);
					var timeGrid = timeGridPool.Get();
					timeGrid.transform.SetParent(timeGridRoot);
					timeGrid.Init(this, currentGridTime, GetColor(gridIndex));
				}
			}
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
			EventManager.Instance.AddListener<T3Time>("Level_OnReset", LevelOnReset);
			EventManager.Instance.AddListener<float>("Level_OnSpeedUpdate", LevelOnSpeedUpdate);
			OnToggleChanged(toggle.isOn);
			LevelOnSpeedUpdate(LevelManager.Instance.LevelSpeed.SpeedRate);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
			EventManager.Instance.RemoveListener<T3Time>("Level_OnReset", LevelOnReset);
			EventManager.Instance.RemoveListener<float>("Level_OnSpeedUpdate", LevelOnSpeedUpdate);
		}
	}
}