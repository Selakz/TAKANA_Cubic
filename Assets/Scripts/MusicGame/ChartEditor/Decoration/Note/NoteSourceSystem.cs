#nullable enable

using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Note
{
	public class NoteSourceSystem : T3System
	{
		private readonly IDataset<ChartComponent> noteDataset;
		private readonly ChartSelectDataset selectDataset;

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new DatasetRegistrar<ChartComponent>(selectDataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataAdded, component =>
				{
					if (component.Model is INote) noteDataset.Add(component);
				}),
			new DatasetRegistrar<ChartComponent>(selectDataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataRemoved, component =>
				{
					if (component.Model is INote) noteDataset.Remove(component);
				})
		};

		public NoteSourceSystem(
			[Key("note-decoration")] IDataset<ChartComponent> noteDataset,
			ChartSelectDataset selectDataset) : base(true)
		{
			this.noteDataset = noteDataset;
			this.selectDataset = selectDataset;
		}
	}
}