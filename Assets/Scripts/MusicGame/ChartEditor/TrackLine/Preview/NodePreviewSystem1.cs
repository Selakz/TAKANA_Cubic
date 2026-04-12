#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.TrackLine.Render;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Speed;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Threading;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Easing;
using T3Framework.Static.Event;
using T3Framework.Static.Movement;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine.Preview
{
	// The code is not very clean but this is not a core system so fine.
	public class NodePreviewSystem1 : HierarchySystem<NodePreviewSystem1>
	{
		// Serializable and Public
		[SerializeField] private ViewPoolInstaller headPluginPoolInstaller;
		[SerializeField] private Color illegalColor = Color.red;
		[SerializeField] private float previewAlpha = 0.25f;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			// The count of preview nodes are usually small, and they are usually all updated in the same frame.
			// So the strategy is when one node is updated, we wait for the next frame to update all preview nodes.
			new DatasetRegistrar<NodeRawInfo>(dataset,
				DatasetRegistrar<NodeRawInfo>.RegisterTarget.DataAdded, info =>
				{
					initRcts.CancelAndReset();
					AddToMap(info);
					UniTask.DelayFrame(1, cancellationToken: poolRcts.Token) // Wait for track decorator to be created.
						.ContinueWith(() =>
						{
							viewPool.Add(info);
							WaitAndInitAllPreview().Forget();
						});
				}),
			new DatasetRegistrar<NodeRawInfo>(dataset,
				DatasetRegistrar<NodeRawInfo>.RegisterTarget.DataUpdated, info =>
				{
					initRcts.CancelAndReset();
					RemoveFromMap(info);
					AddToMap(info);
					if (!viewPool.Contains(info)) return;
					WaitAndInitAllPreview().Forget();
				}),
			new DatasetRegistrar<NodeRawInfo>(dataset,
				DatasetRegistrar<NodeRawInfo>.RegisterTarget.DataRemoved, info =>
				{
					initRcts.CancelAndReset();
					RemoveFromMap(info);
					poolRcts.CancelAndReset();
					viewPool.Remove(info);
					WaitAndInitAllPreview().Forget();
				}),

			new ViewPoolLifetimeRegistrar<NodeRawInfo>(viewPool, handler => new CustomRegistrar(
				() =>
				{
					var info = viewPool[handler]!;
					if (decoratorPool[info.Parent] is not { } trackHandler)
					{
						Debug.LogWarning($"preview node not find parent track decorator, id {info.Parent.Value.Id}");
						return;
					}

					trackHandler.AddPlugin(GetNodeName(info), handler);
				},
				() =>
				{
					var info = viewPool[handler]!;
					handler.RemovePlugin(HeadPluginName, headPluginPool.DefaultTransform);
					headPluginPool.Remove(info);
					if (handler.Parent is { } trackHandler)
					{
						trackHandler.RemovePlugin(GetNodeName(info), viewPool.DefaultTransform);
					}
				}))
		};

		// Private
		[Inject] private readonly IDataset<NodeRawInfo> dataset = default!;
		[Inject] private readonly IViewPool<NodeRawInfo> viewPool = default!;
		[Inject] [Key("head-plugin")] private readonly IViewPool<NodeRawInfo> headPluginPool = default!;
		[Inject] [Key("track-decoration")] private readonly IViewPool<ChartComponent> decoratorPool = default!;
		[Inject] private readonly NotifiableProperty<ISpeed> speed = default!;

		private readonly Dictionary<ChartComponent, List<NodeRawInfo>> previewMap = new();
		private readonly ReusableCancellationTokenSource initRcts = new();
		private readonly ReusableCancellationTokenSource poolRcts = new();

		// Static
		private const string HeadPluginName = "head-plugin";

		// Constructor
		public override void SelfInstall(IContainerBuilder builder)
		{
			base.SelfInstall(builder);
			builder.RegisterViewPool<NodeRawInfo>(headPluginPoolInstaller).Keyed("head-plugin");
		}

		// Defined Functions
		private static string GetNodeName(NodeRawInfo info) => $"PreviewNode{info.Time.Value}-{info.Type.Value}";

		private void AddToMap(NodeRawInfo info)
		{
			if (!previewMap.ContainsKey(info.Parent.Value)) previewMap[info.Parent.Value] = new();
			previewMap[info.Parent.Value].Add(info);
			previewMap[info.Parent.Value].Sort((a, b) => a.Time.Value.CompareTo(b.Time.Value));
		}

		private void RemoveFromMap(NodeRawInfo info)
		{
			if (previewMap.TryGetValue(info.Parent.Value, out var list))
			{
				list.Remove(info);
				if (list.Count == 0) previewMap.Remove(info.Parent.Value);
			}
		}

		public async UniTask WaitAndInitAllPreview()
		{
			initRcts.CancelAndReset();
			await UniTask.DelayFrame(1, cancellationToken: initRcts.Token);
			foreach (var (track, list) in previewMap)
			{
				var model = (track.Model as ITrack)!;
				bool? isPosLeft = null;
				for (var i = 0; i < list.Count; i++)
				{
					var info = list[i];
					// Using linq may be slow and repetitive. But I'm lazy.
					var prevInfo = list.Take(i).LastOrDefault(prev => prev.Type.Value == info.Type.Value);
					var nextInfo = list.Skip(i + 1).FirstOrDefault(next => next.Type.Value == info.Type.Value);
					switch (info.Type.Value)
					{
						case NodeType.Left:
							InitPosNodePreview(info, prevInfo, nextInfo, true);
							break;
						case NodeType.Right:
							InitPosNodePreview(info, prevInfo, nextInfo, false);
							break;
						case NodeType.Pos:
							if (isPosLeft is null)
							{
								// When isPosLeft is null, this info is the first pos info.
								var pos = model.Movement.GetPos(info.Time.Value);
								isPosLeft = info.Node.Value.Position < pos;
							}

							switch (model.Movement)
							{
								case TrackEdgeMovement:
									InitPosNodePreview(info, prevInfo, nextInfo, isPosLeft.Value);
									break;
								case TrackDirectMovement:
									InitPosNodePreview(info, prevInfo, nextInfo, true);
									break;
							}

							break;
						case NodeType.Width:
							InitWidthNodePreview(info);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
		}

		public void InitWidthNodePreview(NodeRawInfo info)
		{
			var track = (info.Parent.Value.Model as ITrack)!;
			bool isLegal = track.Movement is TrackDirectMovement;
			var view = viewPool[info]!.Script<WidthNodeView>();
			InitNodeColor(viewPool[info]!, isLegal);
			var x = track.Movement.GetPos(info.Time.Value);
			var y = (info.Time.Value - track.TimeStart).Second * speed.Value.SpeedRate;
			switch (info.Node.Value)
			{
				case V1EMoveItem easeItem:
					view.Init(easeItem.Ease.GetString(), new(x, y), easeItem.Position);
					break;
				case V1BMoveItem bezierItem:
					view.Init("bezier", new(x, y), bezierItem.Position);
					break;
			}
		}

		public void InitPosNodePreview(
			NodeRawInfo info, NodeRawInfo? prevInfo, NodeRawInfo? nextInfo, bool isFirst)
		{
			var track = (info.Parent.Value.Model as ITrack)!;
			var move = isFirst ? track.Movement.Movement1 : track.Movement.Movement2;
			if (move is not ChartPosMoveList movement) return;

			bool isLegal = true;
			var handler = viewPool[info]!;
			var speedRate = speed.Value.SpeedRate;
			var i = movement.BinarySearch(info.Time);
			if (i >= 0) isLegal = i == 0;
			else i = ~i;

			bool isPrevPreview = i == 0 || movement[i - 1].Key < (prevInfo?.Time ?? T3Time.MinValue);
			var prevItem = isPrevPreview ? prevInfo?.Node.Value : movement[i - 1].Value;
			var prevTime = isPrevPreview ? prevInfo?.Time.Value : movement[i - 1].Key;

			bool isNextPreview = i == movement.Count || movement[i].Key > (nextInfo?.Time ?? T3Time.MaxValue);
			var nextItem = isNextPreview ? nextInfo?.Node.Value : movement[i].Value;
			var nextTime = isNextPreview ? nextInfo?.Time.Value : movement[i].Key;

			// See if this preview needs a head plugin
			if (prevItem is not null && !ReferenceEquals(prevItem, prevInfo?.Node.Value))
			{
				headPluginPool.Add(info);
				var plugin = headPluginPool[info]!;
				handler.AddPlugin(HeadPluginName, plugin);
				var prevX = prevItem.Position;
				var prevY = (prevTime! - track.TimeStart).Value.Second * speedRate;
				var px = info.Node.Value.Position;
				var py = (info.Time.Value - track.TimeStart).Second * speedRate;
				InitNodeColor(plugin, isLegal);
				InitPosCurve(prevItem, plugin, new(prevX - px, prevY - py), new(0, 0));
			}
			else
			{
				handler.RemovePlugin(HeadPluginName, headPluginPool.DefaultTransform);
				headPluginPool.Remove(info);
			}

			// Form this preview's own curve
			var x = info.Node.Value.Position;
			var y = (info.Time.Value - track.TimeStart).Second * speedRate;
			var nextX = nextItem?.Position ?? x;
			var nextY = nextTime is not null ? (nextTime - track.TimeStart).Value.Second * speedRate : y;
			InitNodeColor(handler, isLegal);
			InitPosCurve(info.Node.Value, handler, new(x, y), new(nextX, nextY));
		}

		private void InitNodeColor(PrefabHandler handler, bool isLegal)
		{
			var view = handler.Script<INodeView>();
			view.ColorModifier.Register(c => (isLegal ? c : illegalColor) with { a = previewAlpha }, 1);
		}

		private static void InitPosCurve(IPositionMoveItem<float> item, PrefabHandler handler, Vector2 current,
			Vector2 next)
		{
			switch (item)
			{
				case V1EMoveItem easeItem:
					var easeRenderer = handler.Script<EaseLineRenderer>();
					easeRenderer.Init(easeItem.Ease, current, next);
					break;
				case V1BMoveItem bezierItem:
					var bezierRenderer = handler.Script<BezierLineRenderer>();
					bezierRenderer.Init(bezierItem.StartControlFactor, bezierItem.EndControlFactor,
						current, next);
					break;
			}
		}
	}
}