#nullable enable

using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Level;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using VContainer;
using Yarn.Unity;
using ICommand = MusicGame.ChartEditor.Command.ICommand;

namespace MusicGame.EditorTutorial
{
	public class DraftTaskHandler : HierarchySystem<DraftTaskHandler>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new[]
		{
			dispatcher.Registrar("TryCreateDraftNote", () =>
			{
				ISingleton<InputManager>.Instance.BanAllExcept(
					("EditorBasic", "Pause"),
					("InScreenEdit", "Raycast"),
					("General", "Ctrl"),
					("InScreenEdit", "SwitchCreateType"),
					("InScreenEdit", "Create"),
					("Draft", "ToggleDraftMode"));
				tryCreateDraftNoteCompletionSource?.TrySetCanceled();
				tryCreateDraftNoteCompletionSource = new();
				new TryCreateDraftNoteRegistrar(commandManager, tryCreateDraftNoteCompletionSource).Register();
				return tryCreateDraftNoteCompletionSource.Task;
			}),
			dispatcher.Registrar("TryChangeDraftWidth", () =>
			{
				ISingleton<InputManager>.Instance.BanAllExcept(
					("EditorBasic", "Pause"),
					("InScreenEdit", "Raycast"),
					("General", "Ctrl"),
					("InScreenEdit", "Create"),
					("InScreenEdit", "SwitchCreateType"),
					("HoldEdit", "CreateHoldBetween"),
					("InScreenEdit", "Widen"),
					("InScreenEdit", "WidenToGrid"),
					("InScreenEdit", "Narrow"),
					("InScreenEdit", "NarrowToGrid"),
					("InScreenEdit", "ToLeft"),
					("InScreenEdit", "ToLeftGrid"),
					("InScreenEdit", "ToRight"),
					("InScreenEdit", "ToRightGrid"),
					("InScreenEdit", "ToNext"),
					("InScreenEdit", "ToNextBeat"),
					("InScreenEdit", "ToPrevious"),
					("InScreenEdit", "ToPreviousBeat"),
					("Draft", "ToggleDraftMode"));
				tryChangeDraftWidthCompletionSource?.TrySetCanceled();
				tryChangeDraftWidthCompletionSource = new();
				new TryChangeDraftWidthRegistrar(commandManager, tryChangeDraftWidthCompletionSource).Register();
				return tryChangeDraftWidthCompletionSource.Task;
			}),
			dispatcher.Registrar("TryBindDraftToTrack", () =>
			{
				ISingleton<InputManager>.Instance.BanAllExcept(
					("EditorBasic", "Pause"),
					("InScreenEdit", "Raycast"),
					("General", "Ctrl"),
					("InScreenEdit", "Create"),
					("InScreenEdit", "SwitchCreateType"),
					("HoldEdit", "CreateHoldBetween"),
					("InScreenEdit", "Widen"),
					("InScreenEdit", "WidenToGrid"),
					("InScreenEdit", "Narrow"),
					("InScreenEdit", "NarrowToGrid"),
					("InScreenEdit", "ToLeft"),
					("InScreenEdit", "ToLeftGrid"),
					("InScreenEdit", "ToRight"),
					("InScreenEdit", "ToRightGrid"),
					("InScreenEdit", "ToNext"),
					("InScreenEdit", "ToNextBeat"),
					("InScreenEdit", "ToPrevious"),
					("InScreenEdit", "ToPreviousBeat"),
					("Draft", "AttachNote"),
					("Draft", "ToggleDraftMode"));
				tryBindDraftToTrackCompletionSource?.TrySetCanceled();
				tryBindDraftToTrackCompletionSource = new();
				new TryBindDraftToTrackRegistrar(commandManager, tryBindDraftToTrackCompletionSource).Register();
				return tryBindDraftToTrackCompletionSource.Task;
			}),
			dispatcher.Registrar("TryDraftToNode", () =>
			{
				ISingleton<InputManager>.Instance.BanAllExcept(
					("EditorBasic", "Pause"),
					("InScreenEdit", "Raycast"),
					("General", "Ctrl"),
					("InScreenEdit", "Create"),
					("InScreenEdit", "SwitchCreateType"),
					("HoldEdit", "CreateHoldBetween"),
					("InScreenEdit", "Widen"),
					("InScreenEdit", "WidenToGrid"),
					("InScreenEdit", "Narrow"),
					("InScreenEdit", "NarrowToGrid"),
					("InScreenEdit", "ToLeft"),
					("InScreenEdit", "ToLeftGrid"),
					("InScreenEdit", "ToRight"),
					("InScreenEdit", "ToRightGrid"),
					("InScreenEdit", "ToNext"),
					("InScreenEdit", "ToNextBeat"),
					("InScreenEdit", "ToPrevious"),
					("InScreenEdit", "ToPreviousBeat"),
					("Draft", "TrackAnchor"),
					("Draft", "ToggleDraftMode"));
				tryDraftToNodeCompletionSource?.TrySetCanceled();
				tryDraftToNodeCompletionSource = new();
				new TryDraftToNodeRegistrar(commandManager, tryDraftToNodeCompletionSource).Register();
				return tryDraftToNodeCompletionSource.Task;
			}),
			dispatcher.Registrar("TryDraftToTrack", () =>
			{
				ISingleton<InputManager>.Instance.BanAllExcept(
					("EditorBasic", "Pause"),
					("InScreenEdit", "Raycast"),
					("General", "Ctrl"),
					("InScreenEdit", "Create"),
					("InScreenEdit", "SwitchCreateType"),
					("HoldEdit", "CreateHoldBetween"),
					("InScreenEdit", "Widen"),
					("InScreenEdit", "WidenToGrid"),
					("InScreenEdit", "Narrow"),
					("InScreenEdit", "NarrowToGrid"),
					("InScreenEdit", "ToLeft"),
					("InScreenEdit", "ToLeftGrid"),
					("InScreenEdit", "ToRight"),
					("InScreenEdit", "ToRightGrid"),
					("InScreenEdit", "ToNext"),
					("InScreenEdit", "ToNextBeat"),
					("InScreenEdit", "ToPrevious"),
					("InScreenEdit", "ToPreviousBeat"),
					("Draft", "CreateTrack"),
					("Draft", "ToggleDraftMode"));
				tryDraftToTrackCompletionSource?.TrySetCanceled();
				tryDraftToTrackCompletionSource = new();
				new TryDraftToTrackRegistrar(commandManager, tryDraftToTrackCompletionSource).Register();
				return tryDraftToTrackCompletionSource.Task;
			}),
		};

		// Private
		[Inject] private readonly TutorialTaskDispatcher dispatcher = default!;
		[Inject] private readonly NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private readonly CommandManager commandManager = default!;

		private YarnTaskCompletionSource? tryCreateDraftNoteCompletionSource;
		private YarnTaskCompletionSource? tryChangeDraftWidthCompletionSource;
		private YarnTaskCompletionSource? tryBindDraftToTrackCompletionSource;
		private YarnTaskCompletionSource? tryDraftToNodeCompletionSource;
		private YarnTaskCompletionSource? tryDraftToTrackCompletionSource;

		private class TryCreateDraftNoteRegistrar : IEventRegistrar
		{
			private readonly CommandManager commandManager;
			private readonly YarnTaskCompletionSource completionSource;

			public TryCreateDraftNoteRegistrar(CommandManager commandManager, YarnTaskCompletionSource completionSource)
			{
				this.commandManager = commandManager;
				this.completionSource = completionSource;
			}

			public void Register()
			{
				commandManager.OnAdd += OnCommandAdded;
			}

			public void Unregister()
			{
				commandManager.OnAdd -= OnCommandAdded;
			}

			private void OnCommandAdded(ICommand command)
			{
				if (command is not BatchCommand batch) return;
				foreach (var c in batch.Commands)
				{
					if (c is AddComponentCommand { Component.Model: ISolitaryNote })
					{
						completionSource.TrySetResult();
						Unregister();
					}
				}
			}
		}

		private class TryChangeDraftWidthRegistrar : IEventRegistrar
		{
			private readonly CommandManager commandManager;
			private readonly YarnTaskCompletionSource completionSource;

			private int count = 5;

			public TryChangeDraftWidthRegistrar(CommandManager commandManager,
				YarnTaskCompletionSource completionSource)
			{
				this.commandManager = commandManager;
				this.completionSource = completionSource;
			}

			public void Register()
			{
				commandManager.OnAdd += OnCommandAdded;
			}

			public void Unregister()
			{
				commandManager.OnAdd -= OnCommandAdded;
			}

			private void OnCommandAdded(ICommand command)
			{
				if (command is not BatchCommand batch) return;
				foreach (var c in batch.Commands)
				{
					if (c is UpdateComponentCommand { Component.Model: ISolitaryNote })
					{
						count--;
					}
				}

				if (count <= 0)
				{
					completionSource.TrySetResult();
					Unregister();
				}
			}
		}

		private class TryBindDraftToTrackRegistrar : IEventRegistrar
		{
			private readonly CommandManager commandManager;
			private readonly YarnTaskCompletionSource completionSource;

			private int count = 5;

			public TryBindDraftToTrackRegistrar(CommandManager commandManager,
				YarnTaskCompletionSource completionSource)
			{
				this.commandManager = commandManager;
				this.completionSource = completionSource;
			}

			public void Register()
			{
				commandManager.OnAdd += OnCommandAdded;
			}

			public void Unregister()
			{
				commandManager.OnAdd -= OnCommandAdded;
			}

			private void OnCommandAdded(ICommand command)
			{
				if (command is not BatchCommand batch) return;
				int deleteCount = 0;
				foreach (var c in batch.Commands)
				{
					bool isAdd = c is AddComponentCommand { Component.Model: INote };
					bool isDelete = c is DeleteComponentCommand { RootComponent.Model: ISolitaryNote };
					if (!isAdd && !isDelete) return;
					if (isDelete) deleteCount++;
				}

				count -= deleteCount;
				if (count <= 0)
				{
					completionSource.TrySetResult();
					Unregister();
				}
			}
		}

		private class TryDraftToNodeRegistrar : IEventRegistrar
		{
			private readonly CommandManager commandManager;
			private readonly YarnTaskCompletionSource completionSource;

			private int count = 5;

			public TryDraftToNodeRegistrar(CommandManager commandManager, YarnTaskCompletionSource completionSource)
			{
				this.commandManager = commandManager;
				this.completionSource = completionSource;
			}

			public void Register()
			{
				commandManager.OnAdd += OnCommandAdded;
			}

			public void Unregister()
			{
				commandManager.OnAdd -= OnCommandAdded;
			}

			private void OnCommandAdded(ICommand command)
			{
				if (command is not BatchCommand batch) return;
				int updateMoveListCount = 0;
				int deleteCount = 0;
				foreach (var c in batch.Commands)
				{
					if (c is UpdateMoveListCommand)
					{
						updateMoveListCount++;
					}
					else if (c is DeleteComponentCommand { RootComponent.Model: ISolitaryNote })
					{
						deleteCount++;
					}
					else
					{
						return;
					}
				}

				if (updateMoveListCount != 1) return;
				count -= deleteCount;
				if (count <= 0)
				{
					completionSource.TrySetResult();
					Unregister();
				}
			}
		}

		private class TryDraftToTrackRegistrar : IEventRegistrar
		{
			private readonly CommandManager commandManager;
			private readonly YarnTaskCompletionSource completionSource;

			private int count = 5;

			public TryDraftToTrackRegistrar(CommandManager commandManager, YarnTaskCompletionSource completionSource)
			{
				this.commandManager = commandManager;
				this.completionSource = completionSource;
			}

			public void Register()
			{
				commandManager.OnAdd += OnCommandAdded;
			}

			public void Unregister()
			{
				commandManager.OnAdd -= OnCommandAdded;
			}

			private void OnCommandAdded(ICommand command)
			{
				if (command is not BatchCommand batch) return;
				int deleteCount = 0;
				foreach (var c in batch.Commands)
				{
					bool isAdd = c is AddComponentCommand { Component.Model: ITrack };
					bool isDelete = c is DeleteComponentCommand { RootComponent.Model: ISolitaryNote };
					if (!isAdd && !isDelete) return;
					if (isDelete) deleteCount++;
				}

				count -= deleteCount;
				if (count <= 0)
				{
					completionSource.TrySetResult();
					Unregister();
				}
			}
		}
	}
}