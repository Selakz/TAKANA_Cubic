#nullable enable

using System;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.TrackLine;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Models.Track.Movement;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Static;
using T3Framework.Static.Easing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	public class EditV1EItemContent : MonoBehaviour
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

	public class V1EItemContentRegistrar : IEventRegistrar
	{
		private readonly EditV1EItemContent content;
		private readonly EdgeNodeComponent item;
		private readonly IEventRegistrar[] registrars;

		public V1EItemContentRegistrar(EditV1EItemContent content, EdgeNodeComponent item, CommandManager manager)
		{
			this.content = content;
			this.item = item;
			registrars = new IEventRegistrar[]
			{
				CustomRegistrar.Generic<EventHandler>(
					e => item.OnComponentUpdated += e,
					e => item.OnComponentUpdated -= e,
					(_, _) => UpdateUI()),
				new InputFieldRegistrar(content.TimeInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
					value =>
					{
						if (int.TryParse(value, out int newTime))
						{
							if (newTime == item.Locator.Time) return;
							UpdateMoveListArg arg = new(item.Locator.IsLeft, item.Locator.Time,
								new(newTime, item.Model));
							var command = new UpdateMoveListCommand(arg);
							if (!command.SetInit(item.Locator.Track))
							{
								T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
								return;
							}

							manager.Add(command);
						}
						else
						{
							T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
							content.TimeInputField.SetTextWithoutNotify(item.Locator.Time.ToString());
						}
					}),
				new InputFieldRegistrar(content.PositionInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
					value =>
					{
						if (float.TryParse(value, out float newPosition))
						{
							if (Mathf.Approximately(newPosition, item.Model.Position)) return;
							var time = item.Locator.Time;
							var model = item.Model.SetPosition(newPosition);
							UpdateMoveListArg arg = new(item.Locator.IsLeft, time, new(time, model));
							var command = new UpdateMoveListCommand(arg);
							if (!command.SetInit(item.Locator.Track))
							{
								T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
								return;
							}

							manager.Add(command);
						}
						else
						{
							T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
							content.PositionInputField.SetTextWithoutNotify(item.Model.Position.ToString("0.000"));
						}
					}),
				new InputFieldRegistrar(content.EaseInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
					value =>
					{
						Eases newEase = int.TryParse(value, out var curveInt)
							? curveInt >= 100
								? CurveCalculator.GetEaseByRpeNumber(curveInt - 100)
								: CurveCalculator.GetEaseById(curveInt)
							: CurveCalculator.GetEaseByName(value);
						content.EaseInputField.SetTextWithoutNotify(newEase.GetString());
						if (item.Model is not V1EMoveItem model || newEase == model.Ease) return;
						V1EMoveItem newModel = new(model.Position, newEase);
						var time = item.Locator.Time;
						UpdateMoveListArg arg = new(item.Locator.IsLeft, time, new(time, newModel));
						var command = new UpdateMoveListCommand(arg);
						if (!command.SetInit(item.Locator.Track))
						{
							T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
							return;
						}

						manager.Add(command);
					}),
				new ButtonRegistrar(content.AddButton, () =>
				{
					var newTime = item.Locator.Time + ISingleton<TrackLineSetting>.Instance.AddNodeTimeDistance;
					var model = item.Model.SetPosition(item.Model.Position);
					UpdateMoveListArg arg = new(item.Locator.IsLeft, null, new(newTime, model));
					var command = new UpdateMoveListCommand(arg);
					if (!command.SetInit(item.Locator.Track))
					{
						T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
						return;
					}

					manager.Add(command);
				}),
				new ButtonRegistrar(content.RemoveButton, () =>
				{
					UpdateMoveListArg arg = new(item.Locator.IsLeft, item.Locator.Time, null);
					var command = new UpdateMoveListCommand(arg);
					if (!command.SetInit(item.Locator.Track))
					{
						T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
						return;
					}

					manager.Add(command);
				})
			};
		}

		public void UpdateUI()
		{
			content.TimeInputField.SetTextWithoutNotify(item.Locator.Time.ToString());
			content.PositionInputField.SetTextWithoutNotify(item.Model.Position.ToString("0.000"));
			content.EaseInputField.SetTextWithoutNotify((item.Model as V1EMoveItem)?.Ease.GetString());
			content.transform.localScale = Vector3.one;
			content.BgColorModifier.Register(
				_ => item.Locator.IsLeft
					? ISingleton<TrackLineSetting>.Instance.LeftSideNodeColor
					: ISingleton<TrackLineSetting>.Instance.RightSideNodeColor, 0);
		}

		public void Register()
		{
			foreach (var registrar in registrars) registrar.Register();
			UpdateUI();
		}

		public void Unregister()
		{
			foreach (var registrar in registrars) registrar.Unregister();
		}
	}
}