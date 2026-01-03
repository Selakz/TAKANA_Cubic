#nullable enable

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Speed;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine.Preview
{
	public class CopyPasteNodePreviewSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority moduleId = default!;
		[SerializeField] private PrefabObject nodePrefab = default!;
		[SerializeField] private PositionNodeView leftHeadNode = default!;
		[SerializeField] private PositionNodeView rightHeadNode = default!;
		[SerializeField] private SequencePriority nodeAlphaPriority = default!;
		[SerializeField] private SequencePriority illegalColorPriority = default!;
		[SerializeField] private Color illegalColor;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<int>(previewInfo.CurrentModule, () =>
			{
				foreach (var helper in helpers)
				{
					helper.IsShow = moduleId.Value == previewInfo.CurrentModule;
					helper.UpdateView();
				}
			}),
			new PropertyRegistrar<NodeCopyPastePlugin.PasteMode>(copyPastePlugin.Mode, UpdatePreview),
			new PropertyRegistrar<ChartComponent?>(chartSelectDataset.CurrentSelecting, () =>
			{
				UpdatePreview();
				var component = chartSelectDataset.CurrentSelecting.Value;
				if (component is null) return;
				UniTask.DelayFrame(1).ContinueWith(() =>
				{
					foreach (var moveList in viewPool)
					{
						if (moveList.Locator.Track == component)
						{
							if (moveList.Locator.IsLeft)
							{
								helpers[0].MoveList = moveList;
								helpers[0].RootTransform = viewPool[moveList]!.transform;
							}
							else
							{
								helpers[1].MoveList = moveList;
								helpers[1].RootTransform = viewPool[moveList]!.transform;
							}
						}
					}
				});
			}),
			new PropertyRegistrar<ISpeed>(speed, () =>
			{
				foreach (var helper in helpers) helper.SpeedRate = speed.Value.SpeedRate;
			})
		};

		// Private
		private ModuleInfo previewInfo = default!;
		private NodeCopyPastePlugin copyPastePlugin = default!;
		private ChartSelectDataset chartSelectDataset = default!;
		private IViewPool<EdgePMLComponent> viewPool = default!;
		private NotifiableProperty<ISpeed> speed = default!;
		private StageMouseTimeRetriever timeRetriever = default!;

		private IViewPool<EdgeNodeComponent> nodePool = default!;
		private EdgeNodePreviewHelper[] helpers = default!;
		private readonly List<EdgeNodePreviewData> leftData = new();
		private readonly List<EdgeNodePreviewData> rightData = new();
		private EdgeNodePreviewData? baseData;

		// TODO: Rubbish code for updating NormalPaste. Fix it in the future by updating data in NodeCopyPastePlugin itself.
		private ITrack? baseTrack;
		private T3Time baseTime;

		// Constructor
		[Inject]
		private void Construct(
			ModuleInfo previewInfo,
			IObjectResolver resolver,
			NodeCopyPastePlugin copyPastePlugin,
			ChartSelectDataset chartSelectDataset,
			IViewPool<EdgePMLComponent> viewPool,
			NotifiableProperty<ISpeed> speed,
			StageMouseTimeRetriever timeRetriever)
		{
			this.previewInfo = previewInfo;
			this.copyPastePlugin = copyPastePlugin;
			this.chartSelectDataset = chartSelectDataset;
			this.viewPool = viewPool;
			this.speed = speed;
			this.timeRetriever = timeRetriever;
			nodePool = new ViewPool<EdgeNodeComponent>(resolver, nodePrefab, transform);
			helpers = new EdgeNodePreviewHelper[]
			{
				new(leftData)
				{
					Alpha = 0.25f,
					AlphaPriority = nodeAlphaPriority.Value,
					IllegalColor = illegalColor,
					IllegalColorPriority = illegalColorPriority.Value, HeadNode = leftHeadNode
				},
				new(rightData)
				{
					Alpha = 0.25f,
					AlphaPriority = nodeAlphaPriority.Value,
					IllegalColor = illegalColor,
					IllegalColorPriority = illegalColorPriority.Value, HeadNode = rightHeadNode
				}
			};
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Defined Functions
		private void ClearPreview()
		{
			nodePool.Clear();
			leftData.Clear();
			rightData.Clear();
			baseData = null;
			leftHeadNode.gameObject.SetActive(false);
			leftHeadNode.transform.SetParent(transform);
			rightHeadNode.gameObject.SetActive(false);
			rightHeadNode.transform.SetParent(transform);

			lastOffset = 0;
		}

		private void UpdatePreview()
		{
			var mode = copyPastePlugin.Mode.Value;
			var clipboard = copyPastePlugin.Clipboard;
			ClearPreview();

			switch (mode)
			{
				case NodeCopyPastePlugin.PasteMode.None:
					return;
				case NodeCopyPastePlugin.PasteMode.NormalPaste:
				case NodeCopyPastePlugin.PasteMode.ExactPaste:
					FormPreview();
					break;
			}

			return;

			void FormPreview()
			{
				T3Time minTime = T3Time.MaxValue;
				foreach (var component in clipboard)
				{
					if (!nodePool.Add(component)) continue;
					V1EMoveItem item = (component.Model as V1EMoveItem)!;
					EdgeNodePreviewData data = new(nodePool[component]!.Script<PositionNodeView>())
					{
						Ease = item.Ease,
						Position = item.Position,
						Time = component.Locator.Time
					};
					var list = component.Locator.IsLeft ? leftData : rightData;
					list.Add(data);

					if (data.Time < minTime)
					{
						minTime = data.Time;
						baseData = data;
						baseTrack = component.Locator.Track.Model as ITrack;
						baseTime = data.Time;
					}
				}
			}
		}

		// System Functions
		private float lastOffset = 0;

		void Update()
		{
			if (moduleId.Value != previewInfo.CurrentModule) return;
			if (baseData is null || !timeRetriever.GetMouseTimeStart(out var time)) return;

			// Update Time
			var distance = time - baseData.Time;
			foreach (var data in leftData.Concat(rightData)) data.Time += distance;

			// Update Position
			if (copyPastePlugin.Mode == NodeCopyPastePlugin.PasteMode.NormalPaste &&
			    chartSelectDataset.CurrentSelecting.Value?.Model is ITrack track)
			{
				if (baseTrack is null) return;
				var offset = track.Movement.GetPos(time) - baseTrack.Movement.GetPos(baseTime);
				foreach (var data in leftData.Concat(rightData))
				{
					data.Position += offset - lastOffset;
				}

				lastOffset = offset;
			}
			else lastOffset = 0;

			foreach (var helper in helpers) helper.UpdateView();
		}
	}
}