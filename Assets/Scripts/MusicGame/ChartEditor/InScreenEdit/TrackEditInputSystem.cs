#nullable enable

using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class TrackEditInputSystem : HierarchySystem<TrackEditInputSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Create",
				() =>
				{
					commandManager.Add(new BatchCommand(rawDataset.Select(
							info => new CloneComponentCommand(info.Track, info.Parent.Value!.BelongingChart,
								track => track.Parent = info.Parent)),
						"CreateTrack"));
				}),
			new InputRegistrar("InScreenEdit", "CreateTrack",
				() =>
				{
					if (!timeRetriever.GetMouseTimeStart(out var timeStart) ||
					    !timeRetriever.GetMouseTrackTimeEnd(out var timeEnd) ||
					    !widthRetriever.GetMouseWidth(out var width) ||
					    !widthRetriever.GetMousePosition(out var position)) return;

					float left = position - width / 2, right = position + width / 2;
					var command = service.CreateTrack(timeStart, timeEnd, left, right);
					if (command is not null) commandManager.Add(command);
					commandManager.Add(new BatchCommand(rawDataset.Select(
							info => new CloneComponentCommand(info.Track, info.Parent.Value!.BelongingChart,
								track => track.Parent = info.Parent)),
						"CreateTrack"));
				}),
			new InputRegistrar("TrackEdit", "ConnectTrack", () =>
			{
				if (selectDataset.Count != 2) return;
				var tracks = selectDataset.ToArray();

				var command = new ConnectTrackCommand(tracks[0], tracks[1]);
				if (command.SetInit()) commandManager.Add(command);
			})
		};

		// Private
		private ChartEditSystem service = default!;
		private IDataset<TrackRawInfo> rawDataset = default!;
		private ChartSelectDataset selectDataset = default!;
		private StageMouseTimeRetriever timeRetriever = default!;
		private StageMouseWidthRetriever widthRetriever = default!;
		private CommandManager commandManager = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			ChartEditSystem service,
			IDataset<TrackRawInfo> rawDataset,
			ChartSelectDataset selectDataset,
			StageMouseTimeRetriever timeRetriever,
			StageMouseWidthRetriever widthRetriever,
			CommandManager commandManager)
		{
			this.service = service;
			this.rawDataset = rawDataset;
			this.selectDataset = selectDataset;
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;
			this.commandManager = commandManager;
		}
	}
}