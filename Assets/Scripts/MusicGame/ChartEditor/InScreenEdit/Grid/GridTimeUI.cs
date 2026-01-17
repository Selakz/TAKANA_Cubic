#nullable enable

using System;
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
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class GridTimeUI : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Toggle toggle = default!;
		[SerializeField] private TMP_InputField gridDivisionInputField = default!;
		[SerializeField] private PrefabObject timeGridPrefab = default!;
		[SerializeField] private Transform timeGridRoot = default!;
		[SerializeField] private RectTransform timingBlockRoot = default!;

		public event Action? OnBeforeResetGrid;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				var lastInfo = levelInfo.LastValue;
				if (lastInfo is not null) lastInfo.GetsBpmList().OnBpmListUpdate -= ResetGrid;
				var info = levelInfo.Value;
				if (info is not null)
				{
					info.GetsBpmList().OnBpmListUpdate += ResetGrid;
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
		private IObjectResolver resolver = default!;
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private NotifiableProperty<ITimeRetriever> timeRetriever = default!;
		private IGameAudioPlayer music = default!;
		private NotifiableProperty<ISpeed> speed = default!;
		private ITimeRetriever defaultTimeRetriever = default!;
		private GridTimeRetriever gridTimeRetriever = default!;

		private T3Time currentGridTime;
		private T3Time upHeightTimeIncrement;

		private ObjectPool<TimeGridController> TimeGridPool => timeGridPool ??= new(
			() => timeGridPrefab.Instantiate(resolver, timeGridRoot).GetComponent<TimeGridController>(),
			grid => grid.gameObject.SetActive(true),
			grid => grid.gameObject.SetActive(false),
			grid => Destroy(grid.gameObject));

		private ObjectPool<TimeGridController>? timeGridPool;

		// Defined Functions
		[Inject]
		private void Construct(
			IObjectResolver resolver,
			NotifiableProperty<LevelInfo?> levelInfo,
			NotifiableProperty<ITimeRetriever> timeRetriever,
			IGameAudioPlayer music,
			NotifiableProperty<ISpeed> speed,
			ITimeRetriever defaultTimeRetriever,
			GridTimeRetriever gridTimeRetriever)
		{
			this.resolver = resolver;
			this.levelInfo = levelInfo;
			this.timeRetriever = timeRetriever;
			this.music = music;
			this.speed = speed;
			this.defaultTimeRetriever = defaultTimeRetriever;
			this.gridTimeRetriever = gridTimeRetriever;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		public void ResetGrid()
		{
			if (gridTimeRetriever.BpmList is not null)
			{
				OnBeforeResetGrid?.Invoke();
				currentGridTime = gridTimeRetriever.BpmList.GetFloorTime
					(music.ChartTime, gridTimeRetriever.GridDivision, out _);
			}
		}

		private Color GetColor(int gridIndex)
		{
			int remainder = gridIndex % gridTimeRetriever.GridDivision;
			if (remainder == 0) return ISingleton<InScreenEditSetting>.Instance.BeatColor;
			if (gridTimeRetriever.GridDivision % 3 == 0)
			{
				int[] target = { 1, 2, 4, 5 };
				if (target.Any(i => remainder == gridTimeRetriever.GridDivision * i / 6))
				{
					return ISingleton<InScreenEditSetting>.Instance.TripletColor;
				}
			}

			if (gridTimeRetriever.GridDivision % 2 == 0)
			{
				if (remainder == gridTimeRetriever.GridDivision / 2)
					return ISingleton<InScreenEditSetting>.Instance.QuaverColor;
				if (remainder == gridTimeRetriever.GridDivision / 4 ||
				    remainder == gridTimeRetriever.GridDivision * 3 / 4)
					return ISingleton<InScreenEditSetting>.Instance.SemiQuaverColor;
			}

			return ISingleton<InScreenEditSetting>.Instance.DefaultColor;
		}

		public void ReleaseTimeGrid(TimeGridController timeGrid) => TimeGridPool.Release(timeGrid);

		// System Functions
		void Update()
		{
			if (timeRetriever.Value == gridTimeRetriever && gridTimeRetriever.BpmList is not null)
			{
				while (currentGridTime < music.ChartTime + upHeightTimeIncrement)
				{
					currentGridTime = gridTimeRetriever.BpmList.GetCeilTime
						(currentGridTime, gridTimeRetriever.GridDivision, out var gridIndex);
					var timeGrid = TimeGridPool.Get();
					timeGrid.transform.SetParent(timeGridRoot);
					timeGrid.TimingBlockRoot = timingBlockRoot;
					timeGrid.Init(this, currentGridTime, GetColor(gridIndex));
				}
			}
		}
	}
}