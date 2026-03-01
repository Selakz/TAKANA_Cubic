#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit.Preview
{
	public class NotePreviewSystem : HierarchySystem<NotePreviewSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DatasetRegistrar<NoteRawInfo>(dataset,
				DatasetRegistrar<NoteRawInfo>.RegisterTarget.DataAdded, info =>
				{
					var model = info.GenerateModel();
					model.SetDummy(true);
					model.SetIsEditorOnly(true);
					previewMap[info] = new ChartComponent(model) { Parent = info.Parent };
				}),
			new DatasetRegistrar<NoteRawInfo>(dataset,
				DatasetRegistrar<NoteRawInfo>.RegisterTarget.DataUpdated, info =>
				{
					if (!previewMap.TryGetValue(info, out var component)) return;
					previewMap[info] = info.UpdateModel(component);
					if (service.IsValid(info) is not null) previewMap[info].BelongingChart = null;
				}),
			new DatasetRegistrar<NoteRawInfo>(dataset,
				DatasetRegistrar<NoteRawInfo>.RegisterTarget.DataRemoved, info =>
				{
					if (!previewMap.TryGetValue(info, out var component)) return;
					component.BelongingChart = null;
					previewMap.Remove(info);
				})
		};

		// Private
		private IDataset<NoteRawInfo> dataset = default!;
		private INoteRawInfoService service = default!;

		private readonly Dictionary<NoteRawInfo, ChartComponent> previewMap = new();

		// Constructor
		[Inject]
		private void Construct(
			IDataset<NoteRawInfo> dataset,
			INoteRawInfoService service)
		{
			this.dataset = dataset;
			this.service = service;
		}
	}
}