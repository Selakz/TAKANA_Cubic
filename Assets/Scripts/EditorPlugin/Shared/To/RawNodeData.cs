#nullable enable

using MusicGame.ChartEditor.Decoration.Track;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	public class RawNodeData
	{
		public string type { get; }

		public object track { get; }

		public int time => Component.Time.Value.Milli;

		public bool isLeft { get; }

		public bool isPosition { get; }

		internal TrackNodeComponent Component { get; }

		public RawNodeData(TrackNodeComponent component, ChartApi api)
		{
			type = component.IsEdge ? "Edge" : "Direct";
			track = api.GetComponentSnapshot(component.Track);
			isLeft = component.Type == TrackNodeType.Left;
			isPosition = component.Type == TrackNodeType.Pos;
			Component = component;
		}

		public RawMoveItem getMoveItem() => RawMoveItem.From(Component.Model, time);

		public int getNextTime()
		{
			var nodeTime = Component.Time.Value;
			if (Component.MoveList is not { } list) return nodeTime;
			foreach (var (t, _) in list)
			{
				if (t > nodeTime) return t;
			}

			return nodeTime;
		}
	}
}
