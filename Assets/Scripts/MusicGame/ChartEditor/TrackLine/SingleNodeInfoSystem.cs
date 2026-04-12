#nullable enable

using System;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Easing;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine
{
	/// <summary>
	/// Handles one node, and its type is either <see cref="NodeType.Pos"/> or <see cref="NodeType.Width"/>.
	/// As to how the node fits in edge movement, this system does not care about it.
	/// </summary>
	public class SingleNodeInfoSystem : HierarchySystem<SingleNodeInfoSystem>
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
			new PropertyRegistrar<ChartComponent?>(selectDataset.CurrentSelecting, component =>
			{
				if (component?.Model is not ITrack) dataset.Clear();
				else
				{
					foreach (var info in dataset) info.Parent.Value = component;
				}
			}),
			new PropertyRegistrar<NodeCurveType>(curveType, type =>
			{
				foreach (var info in dataset)
				{
					var position = info.Node.Value.Position;
					info.Node.Value = type switch
					{
						NodeCurveType.Ease => new V1EMoveItem(position, Eases.Unmove),
						NodeCurveType.Bezier => new V1BMoveItem(position),
						_ => info.Node.Value
					};
				}
			}),
			new InputRegistrar("General", "Shift", () =>
			{
				foreach (var info in dataset)
				{
					if (info.Parent.Value.Model is ITrack { Movement: TrackDirectMovement })
					{
						info.Type.Value = info.Type.Value switch
						{
							NodeType.Pos => NodeType.Width,
							NodeType.Width => NodeType.Pos,
							_ => throw new ArgumentOutOfRangeException()
						};
					}
				}
			})
		};

		// Private
		[Inject] private readonly ModuleInfo moduleInfo = default!;
		[Inject] private readonly IDataset<NodeRawInfo> dataset = default!;
		[Inject] private readonly ChartSelectDataset selectDataset = default!;
		[Inject] private readonly NotifiableProperty<NodeCurveType> curveType = default!;
		[Inject] private readonly StageMouseTimeRetriever timeRetriever = default!;
		[Inject] private readonly StageMouseWidthRetriever widthRetriever = default!;

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			dataset.Clear();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			dataset.Clear();
		}

		void Update()
		{
			if (!timeRetriever.GetMouseTimeStart(out var baseTime) ||
			    selectDataset.CurrentSelecting.Value is not { Model: ITrack model } track) return;
			if (dataset.Count == 0)
			{
				dataset.Add(new NodeRawInfo(0, NodeType.Pos, curveType.Value switch
				{
					NodeCurveType.Ease => new V1EMoveItem(0, Eases.Unmove),
					NodeCurveType.Bezier => new V1BMoveItem(0),
					_ => throw new ArgumentOutOfRangeException()
				}, track));
			}

			foreach (var info in dataset)
			{
				info.Time.Value = baseTime;

				// Change "Position" value
				if (info.Type.Value is NodeType.Pos)
				{
					if (!widthRetriever.GetMouseAttachedPosition(out var actualPosition)) continue;
					if (!Mathf.Approximately(info.Node.Value.Position, actualPosition))
					{
						info.Node.Value.Position = actualPosition;
						info.Node.ForceNotify();
					}
				}
				else
				{
					if (model.Movement.Movement2 is not ChartPosMoveList moveList) continue;
					var index = moveList.BinarySearch(info.Time);
					if (index < 0)
					{
						index = ~index;
						var width = info.Node.Value.Position;
						var newWidth = 1f;
						if (index != 0) newWidth = moveList[index - 1].Value.Position;
						if (!Mathf.Approximately(width, newWidth))
						{
							info.Node.Value.Position = newWidth;
							info.Node.ForceNotify();
						}
					}
				}
			}
		}
	}
}