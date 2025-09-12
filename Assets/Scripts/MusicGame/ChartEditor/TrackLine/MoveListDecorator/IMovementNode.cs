using MusicGame.Components.Movement;

namespace MusicGame.ChartEditor.TrackLine
{
	public interface IMovementNode
	{
		public bool IsSelected { get; set; }
		IMoveItem MoveItem { get; }

		IMoveItem ToLeft();

		IMoveItem ToRight();

		IMoveItem ToLeftGrid();

		IMoveItem ToRightGrid();

		IMoveItem ToNext();

		IMoveItem ToPrevious();

		IMoveItem ToNextBeat();

		IMoveItem ToPreviousBeat();
	}
}