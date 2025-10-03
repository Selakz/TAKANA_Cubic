using T3Framework.Runtime;

namespace MusicGame.Components
{
	public interface IColliderView2D
	{
		public Modifier<bool> ColliderEnabledModifier { get; }

		// TODO: Refactor collider size here
	}
}