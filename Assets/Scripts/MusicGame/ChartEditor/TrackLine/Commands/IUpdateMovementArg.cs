using System;
using MusicGame.Components.Movement;

namespace MusicGame.ChartEditor.TrackLine.Commands
{
	public interface IUpdateMovementArg
	{
		public Action<IMovement<float>> DoAction { get; }

		public Action<IMovement<float>> UndoAction { get; }
	}
}