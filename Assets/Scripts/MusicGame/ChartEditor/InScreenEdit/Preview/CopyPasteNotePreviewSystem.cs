#nullable enable

using System.Collections.Generic;
using MusicGame.ChartEditor.InScreenEdit.CopyPaste;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit.Preview
{
	public class CopyPasteNotePreviewSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority moduleId = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<int>(moduleInfo.CurrentModule, () =>
			{
				var currentModule = moduleInfo.CurrentModule.Value;
				ChangePreviewVisibility(currentModule == moduleId.Value);
			}),
			new PropertyRegistrar<CopyPastePlugin.PasteMode>(copyPastePlugin.Mode, UpdatePreview),
			new PropertyRegistrar<ChartComponent?>(dataset.CurrentSelecting, UpdatePreview)
		};

		// Private
		private ModuleInfo moduleInfo = default!;
		private StageMouseTimeRetriever timeRetriever = default!;
		private CopyPastePlugin copyPastePlugin = default!;
		private ChartSelectDataset dataset = default!;

		// Key: the copied component / Value: the preview component corresponding to key
		private readonly Dictionary<ChartComponent, ComponentPreviewHelper> previewMap = new();
		private ComponentPreviewHelper? baseHelper;

		// Defined Functions
		[Inject]
		private void Construct(
			ModuleInfo moduleInfo,
			StageMouseTimeRetriever timeRetriever,
			CopyPastePlugin copyPastePlugin,
			ChartSelectDataset dataset)
		{
			this.moduleInfo = moduleInfo;
			this.timeRetriever = timeRetriever;
			this.copyPastePlugin = copyPastePlugin;
			this.dataset = dataset;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		private void ChangePreviewVisibility(bool isShow)
		{
			foreach (var helper in previewMap.Values) helper.IsShow = isShow;
		}

		private void ClearPreview()
		{
			try
			{
				foreach (var helper in previewMap.Values) helper.Dispose();
				previewMap.Clear();
				baseHelper = null;
			}
			catch
			{
				// ignore
			}
		}

		private void UpdatePreview()
		{
			var mode = copyPastePlugin.Mode.Value;
			var clipboard = copyPastePlugin.Clipboard;
			ClearPreview();
			switch (mode)
			{
				case CopyPastePlugin.PasteMode.None:
					return;
				case CopyPastePlugin.PasteMode.NormalPaste:
					ChartComponent? parent = null;
					if (dataset.CurrentSelecting.Value is { Model: ITrack model } track &&
					    !(track.BelongingChart?.GetsLayersInfo()[model.GetLayerId()]?.IsDecoration ?? true))
					{
						parent = track;
					}

					FormPreview();
					foreach (var helper in previewMap.Values) helper.Parent = parent;
					break;
				case CopyPastePlugin.PasteMode.ExactPaste:
					FormPreview();
					foreach (var (component, helper) in previewMap) helper.Parent = component.Parent;
					break;
			}

			foreach (var helper in previewMap.Values) helper.IsShow = moduleInfo.CurrentModule.Value == moduleId.Value;
			return;

			void FormPreview()
			{
				foreach (var component in clipboard)
				{
					if (component is not { Model: INote model, BelongingChart: not null }) continue;
					var newModel = (IChartSerializable.Clone(model) as INote)!;
					newModel.SetDummy(true);
					newModel.SetIsEditorOnly(true);
					var previewComponent = new ChartComponent(newModel);
					var helper = new ComponentPreviewHelper(previewComponent);
					previewMap[component] = helper;
					if (baseHelper is null || newModel.TimeMin < baseHelper.Component.Model.TimeMin)
					{
						baseHelper = helper;
					}
				}
			}
		}

		// System Functions
		void Update()
		{
			if (baseHelper is null) return;
			if (!timeRetriever.GetMouseTimeStart(out var newTimeJudge)) return;
			var distance = newTimeJudge - baseHelper.Component.Model.TimeMin;
			foreach (var helper in previewMap.Values) helper.Update(distance);
		}
	}
}