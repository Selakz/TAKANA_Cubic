#nullable enable

using System;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.ChartEditor.TrackLine.Render;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Speed;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Preset.Drag;
using T3Framework.Preset.Select;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using T3Framework.Static.Movement;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine
{
	public class BezierDragHelper : DragHelper
	{
		private readonly Camera camera;
		private readonly ITimeRetriever timeRetriever;
		private readonly IWidthRetriever widthRetriever;
		private readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);

		public override int DragThreshold => ISingleton<EditorSetting>.Instance.MouseDragThreshold;

		public override Vector3 CurrentScreenPoint => Input.mousePosition;

		public BezierEditPlugin? Plugin { get; private set; }
		public int Index { get; private set; }
		public ChartComponent? Track { get; private set; }
		public Vector2 NewFactor { get; private set; } = Vector2.zero;

		private BezierEditPlugin? innerPlugin;

		public BezierDragHelper(Camera camera, ITimeRetriever timeRetriever, IWidthRetriever widthRetriever)
		{
			this.camera = camera;
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;
		}

		private ISpeed? speed;

		public void Prepare(BezierEditPlugin plugin, int index, ChartComponent track, ISpeed speed)
		{
			Plugin = plugin;
			innerPlugin = plugin;
			Index = index;
			Track = track;
			this.speed = speed;
		}

		protected override void BeginDragLogic()
		{
			if (innerPlugin is null) CancelDrag();
		}

		protected override void OnDraggingLogic()
		{
			if (innerPlugin is null)
			{
				CancelDrag();
				return;
			}

			var position = ScreenPointToTrackPosition(Track!);
			innerPlugin.UpdateCurve(Index, position);
		}

		protected override void EndDragLogic()
		{
			if (innerPlugin is null) return;
			var position = ScreenPointToTrackPosition(Track!);
			NewFactor = innerPlugin.UpdateCurve(Index, position, true);
			innerPlugin = null;
		}

		protected override void CancelDragLogic()
		{
			innerPlugin?.CancelCurve();
			innerPlugin = null;
		}

		private Vector2 ScreenPointToTrackPosition(ChartComponent track)
		{
			if (track.Model is not ITrack model ||
			    !camera.ScreenToWorldPoint(gamePlane, CurrentScreenPoint, out var gamePoint))
				return Vector2.zero;
			var x = widthRetriever.GetAttachedPosition(gamePoint);
			var time = timeRetriever.GetTimeStart(gamePoint);
			var y = (time - model.TimeStart) * speed!.SpeedRate;
			return new Vector2(x, y);
		}
	}

	public class BezierDragEditSystem : HierarchySystem<BezierDragEditSystem>
	{
		// Serializable and Public
		[SerializeField] private ViewPoolInstaller pluginPoolInstaller = default!;
		[SerializeField] private SequencePriority raycastPriority = default!;
		[SerializeField] private LayerMask layerMask = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			// Add to plugin pool with each selected data in two datasets
			new DatasetRegistrar<EdgeNodeComponent>(edgeDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataAdded, node =>
				{
					// We assume if the node is selected, it must have a view
					if (node.Model is not V1BMoveItem || edgePool[node] is not { } view) return;
					pluginPool.Add(node);
					view.AddPlugin(PluginName, pluginPool[node]!);
					InitPlugin(pluginPool[node]!.Script<BezierEditPlugin>(), node);
				}),
			new DatasetRegistrar<DirectNodeComponent>(directDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataAdded, node =>
				{
					// We assume if the node is selected, it must have a view
					if (!node.Locator.IsPos || node.Model is not V1BMoveItem ||
					    directPool[node] is not { } view) return;
					pluginPool.Add(node);
					view.AddPlugin(PluginName, pluginPool[node]!);
					InitPlugin(pluginPool[node]!.Script<BezierEditPlugin>(), node);
				}),
			new DatasetRegistrar<EdgeNodeComponent>(edgeDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataRemoved, node =>
				{
					if (node.Model is not V1BMoveItem || edgePool[node] is not { } view) return;
					pluginPool.Remove(node);
					view.RemovePlugin(PluginName, pluginPool.DefaultTransform);
				}),
			new DatasetRegistrar<DirectNodeComponent>(directDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataRemoved, node =>
				{
					if (!node.Locator.IsPos || node.Model is not V1BMoveItem ||
					    directPool[node] is not { } view) return;
					pluginPool.Remove(node);
					view.RemovePlugin(PluginName, pluginPool.DefaultTransform);
				}),
			// If raycast the plugin, do update
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", raycastPriority.Value, OnBeginDrag,
				InputActionPhase.Started),
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", raycastPriority.Value, OnEndDrag,
				InputActionPhase.Performed),
			new InputRegistrar("General", "Escape",
				() => dragHelper.CancelDrag())
		};

		// Private
		[Inject] private readonly EdgeNodeSelectDataset edgeDataset = default!;
		[Inject] private readonly DirectNodeSelectDataset directDataset = default!;
		[Inject] private readonly IViewPool<EdgeNodeComponent> edgePool = default!;
		[Inject] private readonly IViewPool<DirectNodeComponent> directPool = default!;
		[Inject] [Key("bezier")] private readonly IViewPool<IComponent<IPositionMoveItem<float>>> pluginPool = default!;
		[Inject] private readonly NotifiableProperty<ISpeed> speed = default!;
		[Inject] private readonly CommandManager commandManager = default!;

		private BezierDragHelper dragHelper = default!;
		private MouseRayMaker rayMaker = default!;
		private readonly RaycastHit[] hitBuffer = new RaycastHit[2];

		// Constructor
		[Inject]
		private void Construct([Key("stage")] Camera levelCamera,
			ITimeRetriever timeRetriever, IWidthRetriever widthRetriever)
		{
			dragHelper = new BezierDragHelper(levelCamera, timeRetriever, widthRetriever);
			rayMaker = new MouseRayMaker(levelCamera);
		}

		public override void SelfInstall(IContainerBuilder builder)
		{
			base.SelfInstall(builder);
			builder.RegisterViewPool<IComponent<IPositionMoveItem<float>>>(pluginPoolInstaller).Keyed("bezier");
		}

		// Static
		private const string PluginName = "bezier-edit";

		// Defined Functions
		private void InitPlugin(BezierEditPlugin plugin, EdgeNodeComponent node)
		{
			if (edgePool[node]?.Script<BezierLineRenderer>() is not { } renderer) return;
			plugin.Init(
				renderer.StartFactorBuffer, renderer.EndFactorBuffer, renderer.CurrentBuffer, renderer.NextBuffer);
		}

		private void InitPlugin(BezierEditPlugin plugin, DirectNodeComponent node)
		{
			if (directPool[node]?.Script<BezierLineRenderer>() is not { } renderer) return;
			plugin.Init(
				renderer.StartFactorBuffer, renderer.EndFactorBuffer, renderer.CurrentBuffer, renderer.NextBuffer);
		}

		private bool OnBeginDrag()
		{
			if (dragHelper.IsDragging.Value) return true;
			// See if raycast the plugin
			Ray ray = rayMaker.GetRay();
			var hitCount = Physics.RaycastNonAlloc(ray.origin, ray.direction, hitBuffer, 100, layerMask);
			for (var i = 0; i < hitCount; i++)
			{
				var hit = hitBuffer[i];
				if (hit.collider.transform.parent.TryGetComponent<PrefabHandler>(out var handler) &&
				    pluginPool[handler] is { } node)
				{
					var plugin = handler.Script<BezierEditPlugin>();
					var index = hit.collider == plugin.ControlPoint1 ? 1 : 2;
					dragHelper.Prepare(plugin, index, node switch
					{
						EdgeNodeComponent edge => edge.Locator.Track,
						DirectNodeComponent direct => direct.Locator.Track,
						_ => throw new NotSupportedException()
					}, speed.Value);
					return !dragHelper.BeginDrag();
				}
			}

			return true;
		}

		private bool OnEndDrag()
		{
			var result = dragHelper.EndDrag();
			if (!result) return true;
			var plugin = dragHelper.Plugin;
			if (pluginPool[plugin!.GetComponent<PrefabHandler>()] is not { } node) return true;
			var index = dragHelper.Index;
			var factor = dragHelper.NewFactor;
			var track = dragHelper.Track!;
			var item = (node.Model.Clone() as V1BMoveItem)!;
			if (index == 1) item.StartControlFactor = factor;
			if (index == 2) item.EndControlFactor = factor;
			var time = node switch
			{
				EdgeNodeComponent edge => edge.Locator.Time, DirectNodeComponent direct => direct.Locator.Time,
				_ => new(0)
			};
			var isFirst = node switch
			{
				EdgeNodeComponent edge => edge.Locator.IsLeft, DirectNodeComponent direct => direct.Locator.IsPos,
				_ => false
			};
			UpdateMoveListArg arg = new(isFirst, time, new(time, item));
			UpdateMoveListCommand command = new(arg);
			if (command.SetInit(track)) commandManager.Add(command);
			return !result;
		}
	}
}