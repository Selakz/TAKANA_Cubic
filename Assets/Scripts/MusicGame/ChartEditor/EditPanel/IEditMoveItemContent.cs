using System;
using MusicGame.Components.Movement;

namespace MusicGame.ChartEditor.EditPanel
{
	public interface IEditMoveItemContent
	{
		public IMoveItem MoveItem { get; set; }

		public event Action<IMoveItem, IMoveItem> OnMoveItemUpdated;
	}
}