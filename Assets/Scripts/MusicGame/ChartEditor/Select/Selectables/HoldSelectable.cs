using MusicGame.Components.Notes;
using MusicGame.Gameplay.SortingOrder;
using UnityEngine;

namespace MusicGame.ChartEditor.Select.Selectables
{
	public class HoldSelectable : SimpleModelSelectable<Hold>
	{
		// Private
		private HoldController holdController;
		private bool isHighlighted = false;

		// Static
		private const int SelectSpritePriority = 50;

		// Defined Functions
		protected override void Highlight()
		{
			if (isHighlighted) return;
			isHighlighted = true;
			holdController.SpriteModifier.Register(
				_ => Resources.Load<Sprite>("Images/EditorTexture/t3hold_highlight"), SelectSpritePriority);
			holdController.HoldStartSpriteModifier.Register(
				_ => Resources.Load<Sprite>("Images/EditorTexture/t3holdstart_highlight"), SelectSpritePriority);
			holdController.SortingOrderModifier.Register(
				_ => SortingOrderManager.SelectedHoldSortingOrder, SelectSpritePriority);
		}

		protected override void Unhighlight()
		{
			if (!isHighlighted) return;
			isHighlighted = false;
			holdController.SpriteModifier.Unregister(SelectSpritePriority);
			holdController.HoldStartSpriteModifier.Unregister(SelectSpritePriority);
			holdController.SortingOrderModifier.Unregister(SelectSpritePriority);
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			holdController = GetComponent<HoldController>();
		}
	}
}