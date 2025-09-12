using MusicGame.Components.Notes.Movement;
using T3Framework.Runtime;

namespace MusicGame.Components.Notes
{
	public interface INote : IComponent
	{
		public T3Time TimeJudge { get; }

		public INoteMovement Movement { get; }
	}
}