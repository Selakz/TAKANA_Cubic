#nullable enable

using System.Linq;
using EditorPlugin.PluginSystem;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Static.Easing;
using T3Framework.Static.Movement;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	public class RawTrackDirectMovement : RawTrackMovement
	{
		private readonly StagingRegistry? registry;

		public override string type => "Direct";

		public RawTrackDirectMovement(ChartComponent component, StagingRegistry? registry) : base(component, registry)
		{
			this.registry = registry;
		}

		public RawMoveItem? getItem(int timeMilli, bool isPosition)
		{
			var list = isPosition ? Movement1 : Movement2;
			if (list.TryGet(new T3Time(timeMilli), out var item)) return RawMoveItem.From(item, timeMilli);
			return null;
		}

		public RawMoveItem[] getItems(bool isPosition)
		{
			var list = isPosition ? Movement1 : Movement2;
			return list.Select(pair => RawMoveItem.From(pair.Value, pair.Key.Milli)).ToArray();
		}

		public bool set(int timeMilli, IPositionMoveItem<float> item, bool isPosition)
		{
			var list = isPosition ? Movement1 : Movement2;
			if (registry is null) return list.Insert(timeMilli, item);
			registry.Add(() =>
			{
				var command = new UpdateMoveListCommand(new UpdateMoveListArg(isPosition, null, new(timeMilli, item)));
				command.SetInit(track);
				return command;
			});
			return true;
		}

		public bool delete(int timeMilli, bool isPosition)
		{
			var list = isPosition ? Movement1 : Movement2;
			if (registry is null) return list.Remove(timeMilli);
			var result = list.TryGet(timeMilli, out _);
			registry.Add(() =>
			{
				var command = new UpdateMoveListCommand(new UpdateMoveListArg(isPosition, timeMilli, null));
				command.SetInit(track);
				return command;
			});
			return result;
		}

		public void insert(int timeMilli, float position, float width)
		{
			if (registry is null)
			{
				Movement.Insert(timeMilli, position, width);
				return;
			}

			registry.Add(() =>
			{
				var command = new UpdateMoveListCommand(new UpdateMoveListArg[]
				{
					new(true, null, new(timeMilli, new V1EMoveItem(position, Eases.Unmove))),
					new(false, null, new(timeMilli, new V1EMoveItem(width, Eases.Unmove)))
				});
				command.SetInit(track);
				return command;
			});
		}
	}
}