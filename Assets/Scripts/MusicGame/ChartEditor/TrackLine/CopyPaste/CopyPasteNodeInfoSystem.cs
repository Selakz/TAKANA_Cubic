#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.InScreenEdit.CopyPaste;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine.CopyPaste
{
	public class CopyPasteNodeInfoSystem : HierarchySystem<CopyPasteNodeInfoSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority moduleId = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<int>(moduleInfo.CurrentModule, id => IsEnabled = id == moduleId.Value)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<PasteMode>(pasteMode, Clear),
			new PropertyRegistrar<ChartComponent?>(selectDataset.CurrentSelecting, _ => Clear()),

			new InputRegistrar("InScreenEdit", "ToLeft",
				() => UpdatePosition(pos => pos - ISingleton<TrackLineSetting>.Instance.NodePositionNudgeDistance)),
			new InputRegistrar("InScreenEdit", "ToRight",
				() => UpdatePosition(pos => pos + ISingleton<TrackLineSetting>.Instance.NodePositionNudgeDistance)),
			new InputRegistrar("InScreenEdit", "ToLeftGrid",
				() =>
				{
					if (widthRetriever.WidthRetriever.Value is not GridWidthRetriever retriever) return;
					UpdatePosition(pos => retriever.GetLeftAttachedPosition(pos));
				}),
			new InputRegistrar("InScreenEdit", "ToRightGrid",
				() =>
				{
					if (widthRetriever.WidthRetriever.Value is not GridWidthRetriever retriever) return;
					UpdatePosition(pos => retriever.GetRightAttachedPosition(pos));
				})
		};

		// Private
		[Inject] private readonly ModuleInfo moduleInfo = default!;
		[Inject] private readonly NotifiableProperty<PasteMode> pasteMode = default!;
		[Inject] private readonly IDataset<NodeRawInfo> dataset = default!;
		[Inject] [Key("clipboard")] private readonly List<NodeRawInfo> clipboard = default!;
		[Inject] private readonly ChartSelectDataset selectDataset = default!;
		[Inject] private readonly StageMouseTimeRetriever timeRetriever = default!;
		[Inject] private readonly StageMouseWidthRetriever widthRetriever = default!;

		private NodeRawInfo? baseInfo;
		private float lastOffset = 0;
		private T3Time? lastTime = null;

		// Defined Functions
		private void Clear()
		{
			dataset.Clear();
			baseInfo = null;
			lastOffset = 0;
			lastTime = null;
		}

		private void UpdatePosition(Func<float, float> newPositionFunc)
		{
			foreach (var data in dataset)
			{
				if (data.Type.Value is NodeType.Width) continue;
				data.Node.Value.Position = newPositionFunc(data.Node.Value.Position);
				data.Node.ForceNotify();
			}
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			Clear();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			Clear();
		}

		void Update()
		{
			if (!timeRetriever.GetMouseTimeStart(out var time) ||
			    selectDataset.CurrentSelecting.Value is not { Model: ITrack model } track) return;
			if (dataset.Count == 0)
			{
				if (clipboard.Count == 0) return;
				baseInfo = clipboard[0];
				foreach (var info in clipboard.Select(item => item.Clone()))
				{
					info.Parent.Value = track;
					dataset.Add(info);
				}
			}

			lastTime ??= baseInfo!.Time.Value;
			var distance = time - lastTime.Value;
			foreach (var data in dataset) data.Time.Value += distance;
			lastTime = time;

			if (pasteMode.Value == PasteMode.NormalPaste && baseInfo?.Parent.Value.Model is ITrack baseTrack)
			{
				var offset = model.Movement.GetPos(time) - baseTrack.Movement.GetPos(baseInfo.Time);
				foreach (var info in dataset.Where(i => i.Type.Value != NodeType.Width))
				{
					var actualOffset = offset - lastOffset;
					if (!Mathf.Approximately(actualOffset, 0))
					{
						info.Node.Value.Position += actualOffset;
						info.Node.ForceNotify();
					}
				}

				lastOffset = offset;
			}
			else lastOffset = 0;
		}
	}
}