using MusicGame.Components.Tracks;
using MusicGame.Gameplay.SortingOrder;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.Select.Selectables
{
	[RequireComponent(typeof(TrackController))]
	public class TrackSelectable : SimpleModelSelectable<Track>
	{
		// Private
		private TrackController trackController;
		private bool isHighlighted = false;

		// Static
		private const int SelectColorPriority = 50;
		private const int SelectSpritePriority = 50;

		// Defined Functions
		protected override void Highlight()
		{
			if (isHighlighted) return;
			isHighlighted = true;
			trackController.ColorModifier.Register(
				_ => ISingletonSetting<SelectSetting>.Instance.TrackSelectedColor, SelectColorPriority);
			trackController.SpriteModifier.Register(
				_ => Resources.Load<Sprite>("Images/EditorTexture/t3track_highlight"), SelectSpritePriority);
			trackController.SortingOrderModifier.Register(
				_ => SortingOrderManager.SelectedTrackSortingOrder, SelectSpritePriority);
		}

		protected override void Unhighlight()
		{
			if (!isHighlighted) return;
			isHighlighted = false;
			trackController.ColorModifier.Unregister(SelectColorPriority);
			trackController.SpriteModifier.Unregister(SelectSpritePriority);
			trackController.SortingOrderModifier.Unregister(SelectSpritePriority);
		}

		// Event Handlers

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			trackController = GetComponent<TrackController>();
		}
	}
}