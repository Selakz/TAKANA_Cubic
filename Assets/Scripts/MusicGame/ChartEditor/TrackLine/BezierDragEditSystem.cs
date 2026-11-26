#nullable enable

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

		public override Vector3 CurrentScreenPoint => Mouse.current.position.ReadValue();

		public BezierEditPlugin? Plugin { get; private set; }
		public int Index { get; private set; }
		public ChartComponent? Track { get; private set; }
		public Vector2 NewFactor { get; private set; } = Vector2.zero;

		private BezierEditPlugin? innerPlugin;
		private ISpeed? speed;

		public BezierDragHelper(Camera camera, ITimeRetriever timeRetriever, IWidthRetriever widthRetriever)
		{
			this.camera = camera;
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;
		}

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
			new DatasetRegistrar<TrackNodeComponent>(nodeSelectDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataAdded, node =>
				{
					if (!node.IsPosition || node.Model is not V1BMoveItem || nodePool[node] is not { } view) return;
					if (pluginPool.Add(node))
					{
						var pluginHandler = pluginPool[node]!;
						view.AddPlugin(PluginName, pluginHandler);
						InitPlugin(pluginHandler.Script<BezierEditPlugin>(), node);
					}
				}),
			new DatasetRegistrar<TrackNodeComponent>(nodeSelectDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataRemoved, node =>
				{
					if (nodePool[node] is not { } view) return;
					if (pluginPool.Remove(node))
					{
						view.RemovePlugin(PluginName, pluginPool.DefaultTransform);
					}
				}),
			new DatasetRegistrar<TrackNodeComponent>(nodeSelectDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataUpdated, node =>
				{
					if (pluginPool[node] is { } pluginHandler)
					{
						InitPlugin(pluginHandler.Script<BezierEditPlugin>(), node);
					}
				}),
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", raycastPriority.Value, OnBeginDrag,
				InputActionPhase.Started),
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", raycastPriority.Value, OnEndDrag,
				InputActionPhase.Performed),
			new InputRegistrar("General", "Escape",
				() => dragHelper.CancelDrag())
		};

		// Private
		[Inject] private readonly TrackNodeSelectDataset nodeSelectDataset = default!;
		[Inject] private readonly IViewPool<TrackNodeComponent> nodePool = default!;
		[Inject] [Key("bezier")] private readonly IViewPool<TrackNodeComponent> pluginPool = default!;
		[Inject] private readonly NotifiableProperty<ISpeed> speed = default!;
		[Inject] private readonly CommandManager commandManager = default!;

		private BezierDragHelper dragHelper = default!;
		private MouseRayMaker rayMaker = default!;
		private readonly RaycastHit[] hitBuffer = new RaycastHit[2];

		// Static
		private const string PluginName = "bezier-edit";

		// Constructor
		[Inject]
		private void Construct(
			[Key("stage")] Camera levelCamera,
			ITimeRetriever timeRetriever,
			IWidthRetriever widthRetriever)
		{
			dragHelper = new BezierDragHelper(levelCamera, timeRetriever, widthRetriever);
			rayMaker = new MouseRayMaker(levelCamera);
		}

		public override void SelfInstall(IContainerBuilder builder)
		{
			base.SelfInstall(builder);
			builder.RegisterViewPool<TrackNodeComponent>(pluginPoolInstaller).Keyed("bezier");
		}

		// Defined Functions
		private void InitPlugin(BezierEditPlugin plugin, TrackNodeComponent node)
		{
			if (nodePool[node]?.Script<BezierLineRenderer>() is not { } renderer) return;
			plugin.Init(
				renderer.StartFactorBuffer, renderer.EndFactorBuffer, renderer.CurrentBuffer, renderer.NextBuffer);
		}

		private bool OnBeginDrag()
		{
			if (dragHelper.IsDragging.Value) return true;
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
					dragHelper.Prepare(plugin, index, node.Track, speed.Value);
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
			var item = (node.Model.Clone() as V1BMoveItem)!;
			if (index == 1) item.StartControlFactor = factor;
			if (index == 2) item.EndControlFactor = factor;
			var time = node.Time.Value;
			UpdateMoveListArg arg = new(node.IsFirst, time, new(time, item));
			UpdateMoveListCommand command = new(arg);
			if (command.SetInit(node.Track)) commandManager.Add(command);
			return !result;
		}
	}
}
