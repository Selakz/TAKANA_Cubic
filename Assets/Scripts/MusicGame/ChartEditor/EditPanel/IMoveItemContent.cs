using MusicGame.Components.Movement;

namespace MusicGame.ChartEditor.EditPanel
{
	public interface IMoveItemContent
	{
		public void Init(IMoveItem moveItem);

		public void Destroy();
	}
}