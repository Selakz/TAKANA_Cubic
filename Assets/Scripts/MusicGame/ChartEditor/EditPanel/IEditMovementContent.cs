using System;
using MusicGame.ChartEditor.Command;
using MusicGame.Components.Movement;
using MusicGame.Components.Tracks;

namespace MusicGame.ChartEditor.EditPanel
{
	public interface IEditMovementContent
	{
		public IMovement<float> Movement { get; set; }

		public bool IsFirst { get; set; }

		public event Action<ISetInitCommand<Track>> OnMovementUpdated;
	}
}