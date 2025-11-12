using T3Framework.Runtime;

namespace MusicGame.Components
{
	public interface IColliderView
	{
		public Modifier<bool> ColliderEnabledModifier { get; }

		// TODO: Refactor collider size here
	}
}