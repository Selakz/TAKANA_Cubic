#nullable enable

using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Speed;
using MusicGame.Models.Track;
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
	public class NodePreviewSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority moduleId = default!;
		[SerializeField] private PositionNodeView previewNode = default!;
		[SerializeField] private PositionNodeView headNode = default!;
		[SerializeField] private SequencePriority nodeAlphaPriority = default!;
		[SerializeField] private SequencePriority illegalColorPriority = default!;
		[SerializeField] private Color illegalColor;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<int>(previewInfo.CurrentModule, () =>
			{
				helper.IsShow = moduleId.Value == previewInfo.CurrentModule;
				helper.UpdateView();
			}),
			new PropertyRegistrar<ChartComponent?>(dataset.CurrentSelecting, () =>
			{
				leftMoveList = rightMoveList = null;
				var component = dataset.CurrentSelecting.Value;
				if (component is null) return;
				UniTask.DelayFrame(1).ContinueWith(() =>
				{
					foreach (var moveList in viewPool)
					{
						if (moveList.Locator.Track == component)
						{
							if (moveList.Locator.IsLeft) leftMoveList = moveList;
							else rightMoveList = moveList;
						}
					}
				});

				CurrentDecorating = null;
			}),
			new PropertyRegistrar<ISpeed>(speed, () => helper.SpeedRate = speed.Value.SpeedRate)
		};

		// Private
		private ModuleInfo previewInfo = default!;
		private ChartSelectDataset dataset = default!;
		private IViewPool<EdgePMLComponent> viewPool = default!;
		private StageMouseTimeRetriever timeRetriever = default!;
		private StageMouseWidthRetriever widthRetriever = default!;

		private EdgeNodePreviewData previewData = default!;
		private EdgeNodePreviewHelper helper = default!;

		private EdgePMLComponent? leftMoveList;
		private EdgePMLComponent? rightMoveList;
		private EdgePMLComponent? currentDecorating;
		private NotifiableProperty<ISpeed> speed = default!;

		private EdgePMLComponent? CurrentDecorating
		{
			set
			{
				if (currentDecorating == value) return;
				currentDecorating = value;
				helper.MoveList = value;
				helper.RootTransform =
					value is not null && viewPool[value] is { } handler ? handler.transform : transform;
			}
		}

		// Defined Functions
		[Inject]
		private void Construct(
			ModuleInfo previewInfo,
			ChartSelectDataset dataset,
			IViewPool<EdgePMLComponent> viewPool,
			NotifiableProperty<ISpeed> speed,
			StageMouseTimeRetriever timeRetriever,
			StageMouseWidthRetriever widthRetriever)
		{
			this.previewInfo = previewInfo;
			this.dataset = dataset;
			this.viewPool = viewPool;
			this.speed = speed;
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;
			previewData = new(previewNode);
			helper = new(new[] { previewData })
			{
				Alpha = 0.25f,
				AlphaPriority = nodeAlphaPriority.Value,
				IllegalColor = illegalColor,
				IllegalColorPriority = illegalColorPriority.Value,
				HeadNode = headNode
			};
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		public static bool IsLeft(ITrack model, T3Time time, float position)
		{
			var trackPos = model.Movement.GetPos(time);
			return position < trackPos;
		}

		// System Functions
		protected override void Awake()
		{
			base.OnEnable();
			previewNode.IsEditable = false;
			headNode.IsEditable = false;
		}

		void Update()
		{
			if (moduleId.Value != previewInfo.CurrentModule) return;
			if (dataset.CurrentSelecting.Value is not { Model: ITrack model }) return;
			if (!timeRetriever.GetMouseTimeStart(out var time)) return;
			if (!widthRetriever.GetMouseAttachedPosition(out var position)) return;

			previewData.Time = time;
			previewData.Position = position;
			CurrentDecorating = IsLeft(model, time, position) ? leftMoveList : rightMoveList;
			helper.UpdateView();
		}
	}
}