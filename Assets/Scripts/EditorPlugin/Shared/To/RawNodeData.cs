#nullable enable

using System;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Static.Movement;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	public class RawNodeData
	{
		public string type { get; }

		public object track { get; }

		public int time { get; }

		public bool isLeft { get; }

		public bool isPosition { get; }

		internal EdgeNodeComponent? EdgeComponent { get; }

		internal DirectNodeComponent? DirectComponent { get; }

		private readonly Func<IPositionMoveItem<float>> getItem;

		public RawNodeData(EdgeNodeComponent component, ChartApi api)
		{
			type = "Edge";
			track = api.GetComponentSnapshot(component.Locator.Track);
			time = component.Locator.Time.Milli;
			isLeft = component.Locator.IsLeft;
			isPosition = false;
			EdgeComponent = component;
			getItem = () => component.Model;
		}

		public RawNodeData(DirectNodeComponent component, ChartApi api)
		{
			type = "Direct";
			track = api.GetComponentSnapshot(component.Locator.Track);
			time = component.Locator.Time.Milli;
			isLeft = false;
			isPosition = component.Locator.IsPos;
			DirectComponent = component;
			getItem = () => component.Model;
		}

		public RawMoveItem getMoveItem() => RawMoveItem.From(getItem(), time);

		public int getNextTime()
		{
			var nodeTime = EdgeComponent?.Locator.Time ?? DirectComponent!.Locator.Time;
			var isFirst = EdgeComponent?.Locator.IsLeft ?? DirectComponent!.Locator.IsPos;
			var nodeTrack = EdgeComponent?.Locator.Track ?? DirectComponent!.Locator.Track;
			var model = (nodeTrack.Model as ITrack)!;
			var list = (ChartPosMoveList)(isFirst ? model.Movement.Movement1 : model.Movement.Movement2)!;
			foreach (var (t, _) in list)
			{
				if (t > nodeTime) return t;
			}

			return nodeTime;
		}
	}
}