#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Level;
using MusicGame.Gameplay.Speed;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class GridTimeUI : HierarchySystem<GridTimeUI>
	{
		// Serializable and Public
		[SerializeField] private Toggle toggle = default!;
		[SerializeField] private TMP_InputField gridDivisionInputField = default!;
		[SerializeField] private PrefabObject timeGridPrefab = default!;
		[SerializeField] private Transform timeGridRoot = default!;
		[SerializeField] private RectTransform timingBlockRoot = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				var lastInfo = levelInfo.LastValue;
				if (lastInfo is not null) lastInfo.GetsBpmList().OnBpmListUpdate -= ResetGrid;
				var info = levelInfo.Value;
				if (info is not null)
				{
					var bpmList = info.GetsBpmList();
					bpmList.OnBpmListUpdate -= ResetGrid;
					bpmList.OnBpmListUpdate += ResetGrid;
					timeRetriever.Value = gridTimeRetriever;
				}
			}),
			new PropertyRegistrar<ISpeed>(speed, () =>
			{
				var speedRate = speed.Value.SpeedRate;
				upHeightTimeIncrement = ISingleton<PlayfieldSetting>.Instance.UpperThreshold / speedRate;
				ResetGrid();
			}),
			new PropertyRegistrar<int>(gridTimeRetriever.GridDivision, () =>
			{
				gridDivisionInputField.SetTextWithoutNotify(gridTimeRetriever.GridDivision.Value.ToString());
				ResetGrid();
			}),
			new InputFieldRegistrar(gridDivisionInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, content =>
			{
				if (int.TryParse(content, out int division)) gridTimeRetriever.GridDivision.Value = division;
			}),
			new PropertyRegistrar<ITimeRetriever>(timeRetriever, () =>
			{
				var isGrid = timeRetriever.Value == gridTimeRetriever;
				timeGridRoot.gameObject.SetActive(isGrid);
				toggle.SetIsOnWithoutNotify(isGrid);
			}),
			new ToggleRegistrar(toggle, isOn => timeRetriever.Value = isOn ? gridTimeRetriever : defaultTimeRetriever),
			CustomRegistrar.Generic<Action>(
				action => music.OnTimeJump += action, action => music.OnTimeJump -= action,
				ResetGrid)
		};

		// Private
		[Inject, Key("stage")] private Camera levelCamera = default!;
		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private NotifiableProperty<ITimeRetriever> timeRetriever = default!;
		[Inject] private IGameAudioPlayer music = default!;
		[Inject] private NotifiableProperty<ISpeed> speed = default!;
		[Inject] private ITimeRetriever defaultTimeRetriever = default!;
		[Inject] private GridTimeRetriever gridTimeRetriever = default!;
		[Inject] private NotifiableProperty<T3Time> timingBlockClickTime = default!;

		private readonly Queue<(T3Time time, TimeGridController view)> activeGrids = new();
		private Camera? timingBlockUiCamera;
		private T3Time currentGridTime;
		private T3Time upHeightTimeIncrement;

		private ObjectPool<TimeGridController> TimeGridPool => timeGridPool ??= new(
			() =>
			{
				var grid = timeGridPrefab.SimpleInstantiate(timeGridRoot).GetComponent<TimeGridController>();
				grid.SetTimingBlockParent(timingBlockRoot);
				grid.TimingBlock.OnClicked += OnTimingBlockClicked;
				return grid;
			},
			grid => grid.gameObject.SetActive(true),
			grid => grid.gameObject.SetActive(false),
			grid =>
			{
				grid.TimingBlock.OnClicked -= OnTimingBlockClicked;
				Destroy(grid.gameObject);
			});

		private ObjectPool<TimeGridController>? timeGridPool;

		// Defined Functions
		private void OnTimingBlockClicked(T3Time time)
		{
			if (!IsEnabled) return;
			timingBlockClickTime.Value = time;
			timingBlockClickTime.AddUpNotify();
		}

		private void ResetGrid()
		{
			while (activeGrids.Count > 0) TimeGridPool.Release(activeGrids.Dequeue().view);
			if (gridTimeRetriever.BpmList is not null)
			{
				currentGridTime = gridTimeRetriever.BpmList.GetFloorTime(
					music.ChartTime, gridTimeRetriever.GridDivision, out _);
			}
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			var rootCanvas = timingBlockRoot.GetComponentInParent<Canvas>()?.rootCanvas;
			timingBlockUiCamera = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
				? rootCanvas.worldCamera
				: null;
		}

		void Update()
		{
			if (timeRetriever.Value != gridTimeRetriever || gridTimeRetriever.BpmList is null) return;

			var chartTime = music.ChartTime;
			var bpmList = gridTimeRetriever.BpmList;
			var division = gridTimeRetriever.GridDivision;
			while (currentGridTime < chartTime + upHeightTimeIncrement)
			{
				currentGridTime = bpmList.GetCeilTime(currentGridTime, division, out var gridIndex);
				if (currentGridTime == T3Time.MaxValue) break;
				var view = TimeGridPool.Get();
				view.SetData(currentGridTime, GetColor(gridIndex));
				activeGrids.Enqueue((currentGridTime, view));
			}

			while (activeGrids.Count > 0 && chartTime > activeGrids.Peek().time)
			{
				TimeGridPool.Release(activeGrids.Dequeue().view);
			}

			var speedRate = speed.Value.SpeedRate;
			foreach (var (time, view) in activeGrids)
			{
				var y = (time - chartTime).Second * speedRate;
				view.SetPositionY(y, GetTimingBlockY(y));
			}

			return;

			float? GetTimingBlockY(float y)
			{
				var worldPos = timeGridRoot.TransformPoint(new(0, y, 0));
				var screenPos = levelCamera.WorldToScreenPoint(worldPos);
				if (screenPos.z <= 0) return null;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(
					timingBlockRoot, screenPos, timingBlockUiCamera, out var localPoint);
				return localPoint.y;
			}

			Color GetColor(int gridIndex)
			{
				int remainder = gridIndex % division;
				if (remainder == 0) return ISingleton<InScreenEditSetting>.Instance.BeatColor;
				if (division % 3 == 0)
				{
					int[] target = { 1, 2, 4, 5 };
					if (target.Any(i => remainder == division * i / 6))
					{
						return ISingleton<InScreenEditSetting>.Instance.TripletColor;
					}
				}

				if (division % 2 == 0)
				{
					if (remainder == division / 2)
						return ISingleton<InScreenEditSetting>.Instance.QuaverColor;
					if (remainder == division / 4 || remainder == division * 3 / 4)
						return ISingleton<InScreenEditSetting>.Instance.SemiQuaverColor;
				}

				return ISingleton<InScreenEditSetting>.Instance.DefaultColor;
			}
		}
	}
}