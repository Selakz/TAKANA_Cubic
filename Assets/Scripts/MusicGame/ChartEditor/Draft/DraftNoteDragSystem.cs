#nullable enable

using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.Draft
{
	public class DraftNoteDragHelper : NoteDragHelper
	{
		private readonly IDraftNoteService draftService;
		private float lastPosition = 0;

		protected override NoteRawInfo? FromComponent(ChartComponent note) => DraftNoteRawInfo.FromComponent(note);

		protected override void BeginDragLogic()
		{
			base.BeginDragLogic();
			lastPosition = BaseModel is ISolitaryNote note ? note.Position : 0;
		}

		protected override void OnDraggingLogic()
		{
			base.OnDraggingLogic();
			var newPosition = draftService.GetMouseAttachedPosition();
			var offset = newPosition - lastPosition;
			foreach (var info in DraggingInfos.Where(info => info is DraftNoteRawInfo).Cast<DraftNoteRawInfo>())
			{
				info.Position.Value += offset;
			}

			lastPosition = newPosition;
		}

		public DraftNoteDragHelper(
			StageMouseTimeRetriever timeRetriever,
			ChartSelectDataset selectDataset,
			CommandManager commandManager,
			IDataset<NoteRawInfo> rawDataset,
			INoteRawInfoService service,
			IDraftNoteService draftService) : base(timeRetriever, selectDataset, commandManager, rawDataset, service)
		{
			this.draftService = draftService;
		}
	}

	public class DraftNoteDragSystem : HierarchySystem<DraftNoteDragSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(draftContainer.IsInDraftMode, isInDraftMode => IsEnabled = isInDraftMode)
		};

		// Private
		[Inject] private DraftContainer draftContainer = default!;
		[Inject] private NoteDragPlugin noteDragPlugin = default!;

		private DraftNoteDragHelper dragHelper = default!;
		private IEventRegistrar? registrar = default!;

		// System Functions
		[Inject]
		private void Construct(
			ChartSelectDataset dataset,
			StageMouseTimeRetriever timeRetriever,
			CommandManager commandManager,
			IDataset<NoteRawInfo> rawDataset,
			INoteRawInfoService service,
			IDraftNoteService draftService)
		{
			dragHelper = new DraftNoteDragHelper(
				timeRetriever, dataset, commandManager, rawDataset, service, draftService);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			registrar = noteDragPlugin.DragHelper.OverrideDragHelper(dragHelper);
			registrar.Register();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			registrar?.Unregister();
		}
	}
}