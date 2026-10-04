#nullable enable

using MusicGame.ChartEditor.TrackLine;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	/// <summary>
	/// One node of the editor's node clipboard, as the plugin side reads it. The movement data is a value
	/// snapshot, so nothing the plugin does with it reaches the clipboard.
	/// </summary>
	public class RawClipboardNodeData
	{
		public string type { get; }

		public int time { get; }

		private readonly NodeRawInfo info;

		public RawClipboardNodeData(NodeRawInfo info)
		{
			this.info = info;
			type = info.Type.Value.ToString();
			time = info.Time.Value.Milli;
		}

		public RawMoveItem getMoveItem() => RawMoveItem.From(info.Node.Value, time);
	}
}
