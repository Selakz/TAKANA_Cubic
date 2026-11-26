#nullable enable

using MusicGame.ChartEditor.TrackLine;
using MusicGame.Models.Track.Movement;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Static;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	public class EditTrackMovementContent : T3MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public Toggle Toggle1 { get; set; } = default!;

		[field: SerializeField]
		public Toggle Toggle2 { get; set; } = default!;

		[SerializeField] private Image toggle1Icon = default!;
		[SerializeField] private Image toggle2Icon = default!;
		[SerializeField] private Sprite leftIcon = default!;
		[SerializeField] private Sprite rightIcon = default!;
		[SerializeField] private Sprite posIcon = default!;
		[SerializeField] private Sprite widthIcon = default!;

		[field: SerializeField]
		public EditMoveListContent MoveList1 { get; set; } = default!;

		[field: SerializeField]
		public EditMoveListContent MoveList2 { get; set; } = default!;

		public bool IsShow1
		{
			get => isShow1;
			set
			{
				isShow1 = value;
				Toggle1.SetIsOnWithoutNotify(value);
				MoveList1.gameObject.SetActive(value);
				Toggle2.SetIsOnWithoutNotify(!value);
				MoveList2.gameObject.SetActive(!value);
			}
		}

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ToggleRegistrar(Toggle1, isOn =>
			{
				if (!isOn) return;
				IsShow1 = true;
			}),
			new ToggleRegistrar(Toggle2, isOn =>
			{
				if (!isOn) return;
				IsShow1 = false;
			})
		};

		// Private
		private bool isShow1 = true;

		// Defined Functions
		public void SetMovementMode(ITrackMovement movement)
		{
			switch (movement)
			{
				case TrackEdgeMovement:
					gameObject.SetActive(true);
					Toggle1.graphic.color = ISingleton<TrackLineSetting>.Instance.SelectedLeftColor;
					Toggle2.graphic.color = ISingleton<TrackLineSetting>.Instance.SelectedRightColor;
					toggle1Icon.sprite = leftIcon;
					toggle2Icon.sprite = rightIcon;
					IsShow1 = isShow1;
					break;
				case TrackDirectMovement:
					gameObject.SetActive(true);
					Toggle1.graphic.color = ISingleton<TrackLineSetting>.Instance.SelectedPosColor;
					Toggle2.graphic.color = ISingleton<TrackLineSetting>.Instance.SelectedWidthColor;
					toggle1Icon.sprite = posIcon;
					toggle2Icon.sprite = widthIcon;
					IsShow1 = isShow1;
					break;
				default:
					gameObject.SetActive(false);
					break;
			}
		}
	}
}
