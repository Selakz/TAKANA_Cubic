#nullable enable

using System.ComponentModel;
using DynamicPanels;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using Yarn.Unity;
using ICommand = MusicGame.ChartEditor.Command.ICommand;

namespace MusicGame.EditorTutorial
{
	public class FirstMeetTaskHandler : HierarchySystem<FirstMeetTaskHandler>
	{
		// Serializable and Public
		[SerializeField] private string tutorialName = string.Empty;
		[Header("ShowBpm"), SerializeField] private DynamicPanelsCanvas rightPanelCanvas = default!;
		[SerializeField] private NotifiableDataContainer<bool> bpmCollapsable = default!;
		[SerializeField] private int bpmTabIndex = 3;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			dispatcher.Registrar("LoadLevel", () =>
			{
				timeRetriever.Value = defaultTimeRetriever;
				var music = Resources.Load<AudioClip>("Tutorial/BuiltInLevel/music");
				var chartText = Resources.Load<TextAsset>("Tutorial/BuiltInLevel/chart").text;
				var chart = ChartInfo.Deserialize(JObject.Parse(chartText));
				var songInfoText = Resources.Load<TextAsset>("Tutorial/BuiltInLevel/songinfo").text;
				var songInfo = SettingConvertHelper.Deserializer.Deserialize<SongInfo>(songInfoText);
				var preferenceText = Resources.Load<TextAsset>("Tutorial/BuiltInLevel/preference").text;
				var preference = SettingConvertHelper.Deserializer.Deserialize<EditorPreference>(preferenceText);
				var info = new LevelInfo
				{
					LevelPath = "BuiltInResource",
					Chart = chart,
					Music = music,
					SongInfo = songInfo,
					Preference = preference,
					Difficulty = preference.Difficulty
				};
				levelInfo.Value = info;
				return YarnTask.CompletedTask;
			}),
			dispatcher.Registrar("ShowBpm", () =>
			{
				timeRetriever.Value = gridTimeRetriever;
				bpmCollapsable.Property.Value = true;
				var panelGroup = rightPanelCanvas.RootPanelGroup;
				if (panelGroup.Count >= 2 && panelGroup[1] is Panel panel && panel.NumberOfTabs > bpmTabIndex - 1)
				{
					panel.ActiveTab = bpmTabIndex;
				}

				return YarnTask.CompletedTask;
			}),
			dispatcher.Registrar("TryCreateTrackAndNote", () =>
			{
				ISingleton<InputManager>.Instance.BanAllExcept(
					("EditorBasic", "Pause"),
					("InScreenEdit", "Raycast"),
					("General", "Ctrl"),
					("InScreenEdit", "Create"),
					("InScreenEdit", "CreateTrack"));
				tryCreateTrackAndNoteCompletionSource?.TrySetCanceled();
				tryCreateTrackAndNoteCompletionSource = new();
				new TryCreateTrackAndNoteRegistrar(levelInfo.Value!.Chart,
					tryCreateTrackAndNoteCompletionSource).Register();
				return tryCreateTrackAndNoteCompletionSource.Task;
			}),
			dispatcher.Registrar("TrySwitchAndCreateHold", () =>
			{
				ISingleton<InputManager>.Instance.BanAllExcept(
					("EditorBasic", "Pause"),
					("InScreenEdit", "Raycast"),
					("General", "Ctrl"),
					("InScreenEdit", "Create"),
					("InScreenEdit", "SwitchCreateType"),
					("HoldEdit", "CreateHoldBetween"));
				trySwitchAndCreateHoldCompletionSource?.TrySetCanceled();
				trySwitchAndCreateHoldCompletionSource = new();
				new TrySwitchAndCreateHoldRegistrar(levelInfo.Value!.Chart, commandManager,
					trySwitchAndCreateHoldCompletionSource).Register();
				return trySwitchAndCreateHoldCompletionSource.Task;
			}),
			dispatcher.Registrar("TryCreateNode", () =>
			{
				ISingleton<InputManager>.Instance.BanAllExcept(
					("EditorBasic", "Pause"),
					("InScreenEdit", "Raycast"),
					("General", "Ctrl"),
					("InScreenEdit", "Create"));
				tryCreateNodeCompletionSource?.TrySetCanceled();
				tryCreateNodeCompletionSource = new();
				new TryCreateNodeRegistrar(edgeNodeDataset,
					tryCreateNodeCompletionSource).Register();
				return tryCreateNodeCompletionSource.Task;
			}),
			dispatcher.Registrar("TrySwitchEase", () =>
			{
				ISingleton<InputManager>.Instance.BanAllExcept(
					("EditorBasic", "Pause"),
					("InScreenEdit", "Raycast"),
					("General", "Ctrl"),
					("CurveSwitch", "ChangeNodeState"),
					("CurveSwitch", "SwitchToSine"),
					("CurveSwitch", "SwitchToQuad"),
					("CurveSwitch", "SwitchToCubic"),
					("CurveSwitch", "SwitchToQuart"),
					("CurveSwitch", "SwitchToQuint"),
					("CurveSwitch", "SwitchToExpo"),
					("CurveSwitch", "SwitchToCirc"),
					("CurveSwitch", "SwitchToBack"),
					("CurveSwitch", "SwitchToElastic"),
					("CurveSwitch", "SwitchToBounce"),
					("CurveSwitch", "CheckCurve"));
				trySwitchEaseCompletionSource?.TrySetCanceled();
				trySwitchEaseCompletionSource = new();
				new TrySwitchEaseRegistrar(edgeNodeDataset, easeId,
					trySwitchEaseCompletionSource).Register();
				return trySwitchEaseCompletionSource.Task;
			}),
		};

		// Private
		[Inject] private readonly TutorialTaskDispatcher dispatcher = default!;
		[Inject] private readonly DialogueRunner dialogueRunner = default!;
		[Inject] private readonly NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private readonly EdgeNodeDataset edgeNodeDataset = default!;
		[Inject, Key("ease-id")] private readonly NotifiableProperty<int> easeId = default!;
		[Inject] private readonly CommandManager commandManager = default!;
		[Inject] private readonly NotifiableProperty<ITimeRetriever> timeRetriever = default!;
		[Inject] private readonly ITimeRetriever defaultTimeRetriever = default!;
		[Inject] private readonly GridTimeRetriever gridTimeRetriever = default!;

		private YarnTaskCompletionSource? tryCreateTrackAndNoteCompletionSource;
		private YarnTaskCompletionSource? trySwitchAndCreateHoldCompletionSource;
		private YarnTaskCompletionSource? tryCreateNodeCompletionSource;
		private YarnTaskCompletionSource? trySwitchEaseCompletionSource;

		// System Functions
		void Start()
		{
			if (ISingleton<TutorialInfo>.Instance.CompletedTutorials.Value.Contains(tutorialName)) return;
			dialogueRunner.StartDialogue(tutorialName);
		}

		// Inner Classes
		private class TryCreateTrackAndNoteRegistrar : IEventRegistrar
		{
			private readonly ChartInfo chart;
			private readonly YarnTaskCompletionSource completionSource;

			private int trackCount = 1;
			private int noteCount = 3;

			public TryCreateTrackAndNoteRegistrar(ChartInfo chart, YarnTaskCompletionSource completionSource)
			{
				this.chart = chart;
				this.completionSource = completionSource;
			}

			public void Register()
			{
				chart.OnComponentAdded += UpdateTarget;
			}

			public void Unregister()
			{
				chart.OnComponentAdded -= UpdateTarget;
			}

			private void UpdateTarget(ChartComponent component)
			{
				if (component.Model is ITrack) trackCount--;
				else if (component.Model is Hit { Type: HitType.Tap } tap && !tap.IsEditorOnly()) noteCount--;
				if (trackCount <= 0 && noteCount <= 0)
				{
					completionSource.TrySetResult();
					Unregister();
				}
			}
		}

		private class TrySwitchAndCreateHoldRegistrar : IEventRegistrar
		{
			private readonly ChartInfo chart;
			private readonly CommandManager commandManager;
			private readonly YarnTaskCompletionSource completionSource;

			private int slideCount = 1;
			private int holdConnectCount = 1;

			public TrySwitchAndCreateHoldRegistrar(ChartInfo chart, CommandManager commandManager,
				YarnTaskCompletionSource completionSource)
			{
				this.chart = chart;
				this.commandManager = commandManager;
				this.completionSource = completionSource;
			}

			public void Register()
			{
				chart.OnComponentAdded += UpdateTarget;
				commandManager.OnAdd += UpdateCommand;
			}

			public void Unregister()
			{
				chart.OnComponentAdded -= UpdateTarget;
				commandManager.OnAdd -= UpdateCommand;
			}

			private void UpdateTarget(ChartComponent component)
			{
				if (component.Model is Hit { Type: HitType.Slide } hit && !hit.IsEditorOnly()) slideCount--;
				if (slideCount <= 0 && holdConnectCount <= 0)
				{
					completionSource.TrySetResult();
					Unregister();
				}
			}

			private void UpdateCommand(ICommand command)
			{
				if (command is CreateHoldBetweenCommand) holdConnectCount--;
				if (slideCount <= 0 && holdConnectCount <= 0)
				{
					completionSource.TrySetResult();
					Unregister();
				}
			}
		}

		private class TryCreateNodeRegistrar : IEventRegistrar
		{
			private readonly EdgeNodeDataset dataset;
			private readonly YarnTaskCompletionSource completionSource;

			private const int LeftNodeCount = 3;
			private const int RightNodeCount = 3;

			public TryCreateNodeRegistrar(EdgeNodeDataset dataset,
				YarnTaskCompletionSource completionSource)
			{
				this.dataset = dataset;
				this.completionSource = completionSource;
			}

			public void Register()
			{
				dataset.OnDataAdded += UpdateTarget;
			}

			public void Unregister()
			{
				dataset.OnDataAdded -= UpdateTarget;
			}

			private void UpdateTarget(EdgeNodeComponent component)
			{
				int left = 0, right = 0;
				foreach (var node in dataset)
				{
					var model = (node.Locator.Track.Model as ITrack)!;
					if (node.Locator.Time != model.TimeMin && node.Locator.Time != model.TimeMax)
					{
						if (node.Locator.IsLeft) left++;
						else right++;
					}
				}

				if (left >= LeftNodeCount && right >= RightNodeCount)
				{
					completionSource.TrySetResult();
					Unregister();
				}
			}
		}

		private class TrySwitchEaseRegistrar : IEventRegistrar
		{
			private readonly EdgeNodeDataset dataset;
			private readonly NotifiableProperty<int> easeId;
			private readonly YarnTaskCompletionSource completionSource;

			private int changeFamilyCount = 1;
			private int easeCount = 5;

			public TrySwitchEaseRegistrar(EdgeNodeDataset dataset, NotifiableProperty<int> easeId,
				YarnTaskCompletionSource completionSource)
			{
				this.dataset = dataset;
				this.easeId = easeId;
				this.completionSource = completionSource;
			}

			public void Register()
			{
				dataset.OnDataUpdated += UpdateTarget;
				easeId.PropertyChanged += UpdateEaseFamily;
			}

			public void Unregister()
			{
				dataset.OnDataUpdated -= UpdateTarget;
				easeId.PropertyChanged -= UpdateEaseFamily;
			}

			private void UpdateTarget(EdgeNodeComponent component)
			{
				easeCount--;
				if (changeFamilyCount <= 0 && easeCount <= 0)
				{
					completionSource.TrySetResult();
					Unregister();
				}
			}

			private void UpdateEaseFamily(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
			{
				changeFamilyCount--;
				if (easeCount <= 0 && changeFamilyCount <= 0)
				{
					completionSource.TrySetResult();
					Unregister();
				}
			}
		}
	}
}