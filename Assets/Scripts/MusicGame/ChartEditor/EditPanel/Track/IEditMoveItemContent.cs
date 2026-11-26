#nullable enable

using System;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.TrackLine;
using MusicGame.ChartEditor.TrackLine.Commands;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.Modifier;
using T3Framework.Static;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	public interface IEditMoveItemContent
	{
		public Image Background { get; set; }

		public TMP_InputField TimeInputField { get; set; }

		public TMP_InputField PositionInputField { get; set; }

		public Button AddButton { get; set; }

		public Button RemoveButton { get; set; }

		public Modifier<Color> BgColorModifier { get; }
	}

	public abstract class MoveItemContentRegistrar : CompositeRegistrar
	{
		private readonly IEditMoveItemContent content;
		protected readonly TrackNodeComponent node;
		protected readonly CommandManager manager;

		protected MoveItemContentRegistrar(
			IEditMoveItemContent content,
			TrackNodeComponent node,
			CommandManager manager)
		{
			this.content = content;
			this.node = node;
			this.manager = manager;
		}

		protected override IEventRegistrar[] InnerRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<EventHandler>(
				e => node.OnComponentUpdated += e,
				e => node.OnComponentUpdated -= e,
				(_, _) => UpdateUI()),
			new InputFieldRegistrar(content.TimeInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
				value =>
				{
					var time = node.Time.Value;
					if (int.TryParse(value, out int newTime))
					{
						if (newTime == time) return;
						UpdateMoveListArg arg = new(node.IsFirst, time, new(newTime, node.Model));
						var command = new UpdateMoveListCommand(arg);
						if (!command.SetInit(node.Track))
						{
							T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
							content.TimeInputField.SetTextWithoutNotify(time.ToString());
							return;
						}

						manager.Add(command);
					}
					else
					{
						T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
						content.TimeInputField.SetTextWithoutNotify(time.ToString());
					}
				}),
			new InputFieldRegistrar(content.PositionInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
				value =>
				{
					if (float.TryParse(value, out float newPosition))
					{
						if (Mathf.Approximately(newPosition, node.Model.Position)) return;
						var time = node.Time.Value;
						var model = node.Model.SetPosition(newPosition);
						UpdateMoveListArg arg = new(node.IsFirst, time, new(time, model));
						var command = new UpdateMoveListCommand(arg);
						if (!command.SetInit(node.Track))
						{
							T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
							content.PositionInputField.SetTextWithoutNotify(node.Model.Position.ToString("0.000"));
							return;
						}

						manager.Add(command);
					}
					else
					{
						T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
						content.PositionInputField.SetTextWithoutNotify(node.Model.Position.ToString("0.000"));
					}
				}),
			new ButtonRegistrar(content.AddButton, () =>
			{
				var newTime = node.Time.Value + ISingleton<TrackLineSetting>.Instance.AddNodeTimeDistance;
				var model = node.Model.Clone();
				UpdateMoveListArg arg = new(node.IsFirst, null, new(newTime, model));
				var command = new UpdateMoveListCommand(arg);
				if (!command.SetInit(node.Track))
				{
					T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
					return;
				}

				manager.Add(command);
			}),
			new ButtonRegistrar(content.RemoveButton, () =>
			{
				UpdateMoveListArg arg = new(node.IsFirst, node.Time.Value, null);
				var command = new UpdateMoveListCommand(arg);
				if (!command.SetInit(node.Track))
				{
					T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
					return;
				}

				manager.Add(command);
			})
		};

		protected virtual void UpdateUI()
		{
			content.TimeInputField.SetTextWithoutNotify(node.Time.Value.ToString());
			content.PositionInputField.SetTextWithoutNotify(node.Model.Position.ToString("0.000"));
			((MonoBehaviour)content).transform.localScale = Vector3.one;
		}

		protected override void Initialize()
		{
			UpdateUI();
			content.BgColorModifier.Register(
				_ => node.Type switch
				{
					TrackNodeType.Left => ISingleton<TrackLineSetting>.Instance.LeftSideNodeColor,
					TrackNodeType.Right => ISingleton<TrackLineSetting>.Instance.RightSideNodeColor,
					TrackNodeType.Pos => ISingleton<TrackLineSetting>.Instance.PosSideNodeColor,
					TrackNodeType.Width => ISingleton<TrackLineSetting>.Instance.WidthSideNodeColor,
					_ => Color.white
				}, 0);
		}

		protected override void Deinitialize()
		{
			content.BgColorModifier.Unregister(0);
		}
	}
}
