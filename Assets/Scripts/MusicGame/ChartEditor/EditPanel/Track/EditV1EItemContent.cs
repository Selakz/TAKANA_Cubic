#nullable enable

using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Models.Track.Movement;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.Modifier;
using T3Framework.Static.Easing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	public class EditV1EItemContent : MonoBehaviour, IEditMoveItemContent
	{
		// Serializable and Public
		[field: SerializeField]
		public Image Background { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField TimeInputField { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField PositionInputField { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField EaseInputField { get; set; } = default!;

		[field: SerializeField]
		public Button AddButton { get; set; } = default!;

		[field: SerializeField]
		public Button RemoveButton { get; set; } = default!;

		public Modifier<Color> BgColorModifier { get; private set; } = default!;

		// Constructor
		[Inject]
		private void Construct()
		{
			var defaultColor = Background.color;
			BgColorModifier = new Modifier<Color>(
				() => Background.color,
				color => Background.color = color,
				_ => defaultColor);
		}
	}

	public class V1EItemContentRegistrar : MoveItemContentRegistrar
	{
		private readonly EditV1EItemContent content;

		protected override IEventRegistrar[] InnerRegistrars
		{
			get
			{
				var innerRegistrars = new IEventRegistrar[]
				{
					new InputFieldRegistrar(content.EaseInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
						value =>
						{
							Eases newEase = int.TryParse(value, out var curveInt)
								? curveInt >= 100
									? CurveCalculator.GetEaseByRpeNumber(curveInt - 100)
									: CurveCalculator.GetEaseById(curveInt)
								: CurveCalculator.GetEaseByName(value);
							content.EaseInputField.SetTextWithoutNotify(newEase.GetString());
							if (node.Model is not V1EMoveItem model || newEase == model.Ease) return;
							var time = node.Time.Value;
							V1EMoveItem newModel = new(model.Position, newEase);
							UpdateMoveListArg arg = new(node.IsFirst, time, new(time, newModel));
							var command = new UpdateMoveListCommand(arg);
							if (!command.SetInit(node.Track))
							{
								T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
								return;
							}

							manager.Add(command);
						})
				};
				return base.InnerRegistrars.Concat(innerRegistrars).ToArray();
			}
		}

		public V1EItemContentRegistrar(
			EditV1EItemContent content,
			TrackNodeComponent node,
			CommandManager manager)
			: base(content, node, manager)
		{
			this.content = content;
		}

		protected override void UpdateUI()
		{
			base.UpdateUI();
			content.EaseInputField.SetTextWithoutNotify((node.Model as V1EMoveItem)?.Ease.GetString());
		}
	}
}
