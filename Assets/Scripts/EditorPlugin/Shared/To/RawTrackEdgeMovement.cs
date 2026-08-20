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
	public class RawTrackEdgeMovement : RawTrackMovement
	{
		private readonly StagingRegistry? registry;

		public override string type => "Edge";

		public RawTrackEdgeMovement(ChartComponent component, StagingRegistry? registry) : base(component, registry)
		{
			this.registry = registry;
		}

		public RawMoveItem? getItem(int timeMilli, bool isLeft)
		{
			var list = isLeft ? Movement1 : Movement2;
			if (list.TryGet(new T3Time(timeMilli), out var item)) return RawMoveItem.From(item, timeMilli);
			return null;
		}

		public RawMoveItem[] getItems(bool isLeft)
		{
			var list = isLeft ? Movement1 : Movement2;
			return list.Select(pair => RawMoveItem.From(pair.Value, pair.Key.Milli)).ToArray();
		}

		public bool set(int timeMilli, IPositionMoveItem<float> item, bool isLeft)
		{
			var list = isLeft ? Movement1 : Movement2;
			if (registry is null) return list.Insert(timeMilli, item);
			registry.Add(() =>
			{
				T3Time? oldTime = list.TryGet(timeMilli, out _) ? timeMilli : null;
				var command = new UpdateMoveListCommand(new UpdateMoveListArg(isLeft, oldTime, new(timeMilli, item)));
				command.SetInit(track);
				return command;
			});
			return true;
		}

		public bool delete(int timeMilli, bool isLeft)
		{
			var list = isLeft ? Movement1 : Movement2;
			if (registry is null) return list.Remove(timeMilli);
			var result = list.TryGet(timeMilli, out _);
			if (!result) return false;
			registry.Add(() =>
			{
				var command = new UpdateMoveListCommand(new UpdateMoveListArg(isLeft, timeMilli, null));
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
				var left = position - width / 2;
				var right = position + width / 2;
				T3Time? leftOldTime = Movement1.TryGet(timeMilli, out _) ? timeMilli : null;
				T3Time? rightOldTime = Movement2.TryGet(timeMilli, out _) ? timeMilli : null;
				var command = new UpdateMoveListCommand(new UpdateMoveListArg[]
				{
					new(true, leftOldTime, new(timeMilli, new V1EMoveItem(left, Eases.Unmove))),
					new(false, rightOldTime, new(timeMilli, new V1EMoveItem(right, Eases.Unmove)))
				});
				command.SetInit(track);
				return command;
			});
		}
	}
}