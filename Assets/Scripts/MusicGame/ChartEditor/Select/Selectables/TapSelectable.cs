using MusicGame.Components.Notes;
using MusicGame.Gameplay.SortingOrder;
using UnityEngine;

namespace MusicGame.ChartEditor.Select.Selectables
{
	// Unity is good to annoy you when you find you can't use MonoBehaviour with generic type and even have to create a single file for every implementation like this.
	public class TapSelectable : SimpleModelSelectable<Tap>
	{
		// Private
		private TapController tapController;
		private bool isHighlighted = false;

		// Static
		private const int SelectSpritePriority = 50;

		// Defined Functions
		protected override void Highlight()
		{
			if (isHighlighted) return;
			isHighlighted = true;
			tapController.SpriteModifier.Register(
				_ => Resources.Load<Sprite>("Images/EditorTexture/t3tap_highlight"), SelectSpritePriority);
			tapController.SortingOrderModifier.Register(
				_ => SortingOrderManager.SelectedTapSortingOrder, SelectSpritePriority);
		}

		protected override void Unhighlight()
		{
			if (!isHighlighted) return;
			isHighlighted = false;
			tapController.SpriteModifier.Unregister(SelectSpritePriority);
			tapController.SortingOrderModifier.Unregister(SelectSpritePriority);
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			tapController = GetComponent<TapController>();
		}
	}
}