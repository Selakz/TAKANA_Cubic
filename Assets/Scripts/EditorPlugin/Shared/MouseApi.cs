#nullable enable

using System;
using MusicGame.ChartEditor.InScreenEdit;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared
{
	public class MouseApi : IDisposable
	{
		private readonly StageMouseTimeRetriever timeRetriever;
		private readonly StageMouseWidthRetriever widthRetriever;

		internal MouseApi(StageMouseTimeRetriever timeRetriever, StageMouseWidthRetriever widthRetriever)
		{
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;
		}

		public void Dispose()
		{
		}

		public int? getTimeStart() => timeRetriever.GetMouseTimeStart(out var time) ? time.Milli : null;

		public int? getHoldTimeEnd() => timeRetriever.GetMouseHoldTimeEnd(out var time) ? time.Milli : null;

		public int? getTrackTimeEnd() => timeRetriever.GetMouseTrackTimeEnd(out var time) ? time.Milli : null;

		public float? getWidth() => widthRetriever.GetMouseWidth(out var width) ? width : null;

		public float? getPosition() => widthRetriever.GetMousePosition(out var position) ? position : null;

		public float? getAttachedPosition() => widthRetriever.GetMouseAttachedPosition(out var position) ? position : null;
	}
}