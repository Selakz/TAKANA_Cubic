#nullable enable

using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit.Preview
{
	public class NotePreviewSystem : T3MonoBehaviour, ISelfInstaller
	{
		public enum PreviewState
		{
			NotOfModule,
			Reset,
			Illegal,
			Legal
		}

		// Serializable and Public
		[SerializeField] private SequencePriority moduleId = default!;

		public ChartComponent? PreviewComponent { get; private set; }

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<int>(previewInfo.CurrentModule, NotifyStateChange),
			new PropertyRegistrar<ChartComponent?>(dataset.CurrentSelecting, NotifyStateChange),
			new PropertyRegistrar<T3Flag>(inputSystem.NoteType, NotifyStateChange)
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private StageMouseTimeRetriever timeRetriever = default!;
		private ModuleInfo previewInfo = default!;
		private ChartSelectDataset dataset = default!;
		private ChartEditInputSystem inputSystem = default!;

		private readonly IClassifier<T3Flag> classifier = new T3ChartClassifier();
		private PreviewState state = PreviewState.NotOfModule;
		private ChartComponent? previewCache;

		// Defined Functions
		[Inject]
		private void Construct(
			NotifiableProperty<LevelInfo?> levelInfo,
			StageMouseTimeRetriever timeRetriever,
			ModuleInfo previewInfo,
			ChartSelectDataset dataset,
			ChartEditInputSystem inputSystem)
		{
			this.levelInfo = levelInfo;
			this.timeRetriever = timeRetriever;
			this.previewInfo = previewInfo;
			this.dataset = dataset;
			this.inputSystem = inputSystem;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		private void NotifyStateChange()
		{
			var module = previewInfo.CurrentModule.Value;
			if (module != moduleId.Value)
			{
				CancelPreview();
				state = PreviewState.NotOfModule;
			}
			else
			{
				state = PreviewState.Reset;
			}
		}

		private bool IsNoteLegal()
		{
			if (dataset.CurrentSelecting.Value is not { Model : ITrack model }) return false;
			if (!timeRetriever.GetMouseTimeStart(out var newTimeJudge)) return false;
			if (newTimeJudge < model.TimeStart || newTimeJudge > model.TimeEnd) return false;
			return true;
		}

		private void UpdatePreview()
		{
			if (PreviewComponent is not { Model: INote model } note) return;
			bool needUpdate = false;
			if (timeRetriever.GetMouseTimeStart(out var newTimeJudge) &&
			    newTimeJudge != model.TimeJudge)
			{
				var distance = newTimeJudge - model.TimeJudge;
				if (!note.IsWithinParentRange(distance))
				{
					CancelPreview();
					return;
				}

				needUpdate = true;
				model.Nudge(distance);
			}

			if (model is Hold hold && timeRetriever.GetMouseHoldTimeEnd(out var newTimeEnd) &&
			    newTimeEnd != hold.TimeEnd)
			{
				newTimeEnd = Mathf.Min(newTimeEnd, note.Parent?.Model.TimeMax ?? T3Time.MaxValue);
				var distance = newTimeEnd - hold.TimeEnd;
				needUpdate = true;
				hold.NudgeEnd(distance);
			}

			if (needUpdate)
			{
				note.UpdateNotify();
			}
		}

		private void ResetPreview()
		{
			CancelPreview();
			if (levelInfo.Value?.Chart is not { } chart) return;
			if (dataset.CurrentSelecting.Value is not { Model: ITrack model } track ||
			    chart.GetsLayersInfo()[model.GetLayerId()] is { IsDecoration: true }) return;
			if (!timeRetriever.GetMouseTimeStart(out var newTimeJudge)) return;
			var flag = inputSystem.NoteType.Value;
			if (previewCache is not null && classifier.IsSubType(flag, classifier.Classify(previewCache)))
			{
				if (previewCache.Model is not INote note) return;
				var distance = newTimeJudge - note.TimeJudge;
				note.Nudge(distance);
				if (note is Hold hold)
				{
					if (!timeRetriever.GetMouseHoldTimeEnd(out var newTimeEnd)) return;
					newTimeEnd = Mathf.Min(newTimeEnd, model.TimeMax);
					var endDistance = newTimeEnd - hold.TimeEnd;
					hold.NudgeEnd(endDistance);
				}

				previewCache.BelongingChart = chart;
				PreviewComponent = previewCache;
				PreviewComponent.Parent = track;
			}
			else
			{
				previewCache = null;
				INote? note = null;
				switch (flag)
				{
					case T3Flag.Tap:
						note = new Hit(newTimeJudge, HitType.Tap);
						break;
					case T3Flag.Slide:
						note = new Hit(newTimeJudge, HitType.Slide);
						break;
					case T3Flag.Hold:
						if (!timeRetriever.GetMouseHoldTimeEnd(out var newTimeEnd)) return;
						newTimeEnd = Mathf.Min(newTimeEnd, model.TimeMax);
						note = new Hold(newTimeJudge, newTimeEnd);
						break;
					default:
						break;
				}

				if (note is not null)
				{
					note.SetDummy(true);
					note.SetIsEditorOnly(true);
					PreviewComponent = chart.AddComponent(note);
					PreviewComponent.Parent = track;
				}
			}
		}

		private void CancelPreview()
		{
			if (PreviewComponent is not null)
			{
				PreviewComponent.BelongingChart = null;
				PreviewComponent.Parent = null;
			}

			previewCache = PreviewComponent;
			PreviewComponent = null;
		}

		// System Functions
		void LateUpdate()
		{
			switch (state)
			{
				case PreviewState.NotOfModule: // To this state only by notify
					break;
				case PreviewState.Reset: // To this state only by notify
					if (IsNoteLegal())
					{
						ResetPreview();
						state = PreviewState.Legal;
					}
					else
					{
						CancelPreview();
						state = PreviewState.Illegal;
					}

					break;
				case PreviewState.Illegal: // Enter this state should cancel first
					if (IsNoteLegal())
					{
						ResetPreview();
						state = PreviewState.Legal;
					}

					break;
				case PreviewState.Legal: // Enter this state should reset first
					if (IsNoteLegal())
					{
						if (PreviewComponent is null) ResetPreview();
						UpdatePreview();
					}
					else
					{
						CancelPreview();
						state = PreviewState.Illegal;
					}

					break;
			}
		}
	}
}