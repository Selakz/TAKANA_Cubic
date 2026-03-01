#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Movement;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.Utility.UI
{
	public class BottomToolkitPanelUI : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private RectTransform panelRoot = default!;
		[SerializeField] private Button popupButton = default!;
		[SerializeField] private Image popupIcon = default!;
		[SerializeField] private FloatMovementContainer popupMovement = default!;
		[SerializeField] private FloatMovementContainer closeMovement = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(popupButton, () =>
			{
				isPopped = !isPopped;
				var movement = isPopped ? popupMovement : closeMovement;
				movement.Move(
					() => panelRoot.anchoredPosition.y,
					value => panelRoot.anchoredPosition = panelRoot.anchoredPosition with { y = value });
				popupIcon.transform.localRotation = Quaternion.Euler(0, 0, isPopped ? 180 : 0);
			})
		};

		// Private
		private bool isPopped = false;
	}
}