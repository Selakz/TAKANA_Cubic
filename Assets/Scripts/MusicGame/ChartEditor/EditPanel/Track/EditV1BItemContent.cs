#nullable enable

using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track.Movement;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.Modifier;
using T3Framework.Static.Movement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	public class EditV1BItemContent : MonoBehaviour, IEditMoveItemContent
	{
		// Serializable and Public
		[field: SerializeField]
		public Image Background { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField TimeInputField { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField PositionInputField { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField T1InputField { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField X1InputField { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField T2InputField { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField X2InputField { get; set; } = default!;

		[field: SerializeField]
		public Button AddButton { get; set; } = default!;

		[field: SerializeField]
		public Button RemoveButton { get; set; } = default!;

		public Modifier<Color> BgColorModifier { get; private set; } = default!;

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

	public class V1BItemContentRegistrar : MoveItemContentRegistrar
	{
		private readonly EditV1BItemContent content;

		protected override IEventRegistrar[] InnerRegistrars
		{
			get
			{
				var innerRegistrars = new IEventRegistrar[]
				{
					new InputFieldRegistrar(content.T1InputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
						value => DoUpdateCommand(value, 0, content.T1InputField)),
					new InputFieldRegistrar(content.T1InputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
						value => DoUpdateCommand(value, 1, content.X1InputField)),
					new InputFieldRegistrar(content.T1InputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
						value => DoUpdateCommand(value, 2, content.T2InputField)),
					new InputFieldRegistrar(content.T1InputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
						value => DoUpdateCommand(value, 3, content.X2InputField)),
				};
				return base.InnerRegistrars.Concat(innerRegistrars).ToArray();
			}
		}

		public V1BItemContentRegistrar(EditV1BItemContent content, ChartComponent track, T3Time time,
			IComponent<IPositionMoveItem<float>> item, bool isFirst, CommandManager manager, bool isLrPos)
			: base(content, track, time, item, isFirst, manager, isLrPos)
		{
			this.content = content;
		}

		protected override void Initialize()
		{
			base.Initialize();
			if (item.Model is not V1BMoveItem model) return;
			content.T1InputField.SetTextWithoutNotify(model.StartControlFactor.x.ToString("0.00"));
			content.X1InputField.SetTextWithoutNotify(model.StartControlFactor.y.ToString("0.00"));
			content.T2InputField.SetTextWithoutNotify(model.EndControlFactor.x.ToString("0.00"));
			content.X2InputField.SetTextWithoutNotify(model.EndControlFactor.y.ToString("0.00"));
		}

		private void DoUpdateCommand(string value, int index, TMP_InputField inputField)
		{
			if (item.Model is not V1BMoveItem model) return;
			if (float.TryParse(value, out var result) && !Mathf.Approximately(result, index switch
			    {
				    0 => model.StartControlFactor.x,
				    1 => model.StartControlFactor.y,
				    2 => model.EndControlFactor.x,
				    3 => model.EndControlFactor.y,
				    _ => 0
			    }))
			{
				result = Mathf.Clamp01(result);
				var start = model.StartControlFactor;
				var end = model.EndControlFactor;
				switch (index)
				{
					case 0:
						start.x = result;
						break;
					case 1:
						start.y = result;
						break;
					case 2:
						end.x = result;
						break;
					case 3:
						end.y = result;
						break;
					default:
						break;
				}

				var newModel = new V1BMoveItem(item.Model.Position, start, end);
				UpdateMoveListArg arg = new(isFirst, time, new(time, newModel));
				var command = new UpdateMoveListCommand(arg);
				if (!command.SetInit(track))
				{
					T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
					return;
				}

				manager.Add(command);
			}
			else
			{
				inputField.SetTextWithoutNotify((index switch
				{
					0 => model.StartControlFactor.x,
					1 => model.StartControlFactor.y,
					2 => model.EndControlFactor.x,
					3 => model.EndControlFactor.y,
					_ => 0
				}).ToString("0.00"));
			}
		}
	}
}