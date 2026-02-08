#nullable enable

using System;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.TrackLine;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Chart;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.Modifier;
using T3Framework.Static;
using T3Framework.Static.Movement;
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
		protected readonly IComponent<IPositionMoveItem<float>> item;
		protected readonly ChartComponent track;
		protected readonly T3Time time;
		protected readonly bool isFirst;
		protected readonly CommandManager manager;
		private readonly bool isLrPos;

		protected MoveItemContentRegistrar(
			IEditMoveItemContent content, ChartComponent track, T3Time time,
			IComponent<IPositionMoveItem<float>> item, bool isFirst, CommandManager manager, bool isLrPos)
		{
			this.content = content;
			this.track = track;
			this.time = time;
			this.item = item;
			this.isFirst = isFirst;
			this.manager = manager;
			this.isLrPos = isLrPos;
		}

		protected override IEventRegistrar[] InnerRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<EventHandler>(
				e => item.OnComponentUpdated += e,
				e => item.OnComponentUpdated -= e,
				(_, _) => Initialize()),
			new InputFieldRegistrar(content.TimeInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
				value =>
				{
					if (int.TryParse(value, out int newTime))
					{
						if (newTime == time) return;
						UpdateMoveListArg arg = new(isFirst, time, new(newTime, item.Model));
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
						T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
						content.TimeInputField.SetTextWithoutNotify(time.ToString());
					}
				}),
			new InputFieldRegistrar(content.PositionInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
				value =>
				{
					if (float.TryParse(value, out float newPosition))
					{
						if (Mathf.Approximately(newPosition, item.Model.Position)) return;
						var model = item.Model.SetPosition(newPosition);
						UpdateMoveListArg arg = new(isFirst, time, new(time, model));
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
						T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
						content.PositionInputField.SetTextWithoutNotify(item.Model.Position.ToString("0.000"));
					}
				}),
			new ButtonRegistrar(content.AddButton, () =>
			{
				var newTime = time + ISingleton<TrackLineSetting>.Instance.AddNodeTimeDistance;
				var model = item.Model.SetPosition(item.Model.Position);
				UpdateMoveListArg arg = new(isFirst, null, new(newTime, model));
				var command = new UpdateMoveListCommand(arg);
				if (!command.SetInit(track))
				{
					T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
					return;
				}

				manager.Add(command);
			}),
			new ButtonRegistrar(content.RemoveButton, () =>
			{
				UpdateMoveListArg arg = new(isFirst, time, null);
				var command = new UpdateMoveListCommand(arg);
				if (!command.SetInit(track))
				{
					T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
					return;
				}

				manager.Add(command);
			})
		};

		protected override void Initialize()
		{
			content.TimeInputField.SetTextWithoutNotify(time.ToString());
			content.PositionInputField.SetTextWithoutNotify(item.Model.Position.ToString("0.000"));
			((MonoBehaviour)content).transform.localScale = Vector3.one;
			content.BgColorModifier.Register(
				_ => isFirst
					? isLrPos
						? ISingleton<TrackLineSetting>.Instance.LeftSideNodeColor
						: ISingleton<TrackLineSetting>.Instance.PosSideNodeColor
					: isLrPos
						? ISingleton<TrackLineSetting>.Instance.RightSideNodeColor
						: ISingleton<TrackLineSetting>.Instance.WidthSideNodeColor, 0);
		}

		protected override void Deinitialize()
		{
			content.BgColorModifier.Unregister(0);
		}
	}
}