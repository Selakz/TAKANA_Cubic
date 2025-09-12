using MusicGame.Components.Notes;
using MusicGame.Gameplay.SortingOrder;
using UnityEngine;

namespace MusicGame.ChartEditor.Select.Selectables
{
	public class SlideSelectable : SimpleModelSelectable<Slide>
	{
		// Private
		private SlideController slideController;
		private bool isHighlighted = false;

		// Static
		private const int SelectSpritePriority = 50;

		// Defined Functions
		protected override void Highlight()
		{
			if (isHighlighted) return;
			isHighlighted = true;
			slideController.SpriteModifier.Register(
				_ => Resources.Load<Sprite>("Images/EditorTexture/t3slide_highlight"), SelectSpritePriority);
			slideController.SortingOrderModifier.Register(
				_ => SortingOrderManager.SelectedSlideSortingOrder, SelectSpritePriority);
		}

		protected override void Unhighlight()
		{
			if (!isHighlighted) return;
			isHighlighted = false;
			slideController.SpriteModifier.Unregister(SelectSpritePriority);
			slideController.SortingOrderModifier.Unregister(SelectSpritePriority);
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			slideController = GetComponent<SlideController>();
		}
	}
}