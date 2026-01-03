#nullable enable

using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Note
{
	public class NoteDecoratorSystem : T3System
	{
		private readonly IDataset<ChartComponent> noteDataset;
		private readonly IViewPool<ChartComponent> decoratorPool;

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new DatasetRegistrar<ChartComponent>(noteDataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataAdded,
				component => decoratorPool.Add(component)),
			new DatasetRegistrar<ChartComponent>(noteDataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataRemoved,
				component => decoratorPool.Remove(component))
		};

		public NoteDecoratorSystem(
			[Key("note-decoration")] IDataset<ChartComponent> noteDataset,
			[Key("note-decoration")] IViewPool<ChartComponent> decoratorPool) : base(true)
		{
			this.noteDataset = noteDataset;
			this.decoratorPool = decoratorPool;
		}
	}
}