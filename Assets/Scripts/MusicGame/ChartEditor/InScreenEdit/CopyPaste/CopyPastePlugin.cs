#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit.CopyPaste
{
	public class CopyPastePlugin : T3MonoBehaviour, ISelfInstaller
	{
		public enum PasteMode
		{
			None,
			NormalPaste,
			ExactPaste
		}

		// Serializable and Public
		[SerializeField] private SequencePriority chartEditPriority = default!;

		public IReadOnlyList<ChartComponent> Clipboard => clipboard;

		public NotifiableProperty<PasteMode> Mode { get; private set; } = new(PasteMode.None);

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Cut", chartEditPriority.Value,
				() =>
				{
					var ret = CopyToClipboard();
					currentPasteHandler.Cut();
					return ret;
				}),
			new InputRegistrar("InScreenEdit", "Copy", chartEditPriority.Value,
				CopyToClipboard),
			new InputRegistrar("InScreenEdit", "Paste", chartEditPriority.Value,
				() =>
				{
					if (clipboard.Count == 0) return true;
					Mode.Value = PasteMode.NormalPaste;
					return false;
				}),
			new InputRegistrar("InScreenEdit", "ExactPaste", chartEditPriority.Value,
				() =>
				{
					if (clipboard.Count == 0) return true;
					Mode.Value = PasteMode.ExactPaste;
					return false;
				}),
			new InputRegistrar("InScreenEdit", "CheckClipboard", chartEditPriority.Value,
				() =>
				{
					CheckClipboard();
					return false;
				}),
			new InputRegistrar("InScreenEdit", "Create", chartEditPriority.Value,
				() =>
				{
					switch (Mode.Value)
					{
						case PasteMode.None:
							return true;
						case PasteMode.NormalPaste:
							Paste();
							break;
						case PasteMode.ExactPaste:
							ExactPaste();
							break;
					}

					Mode.Value = PasteMode.None;
					return false;
				}),
			new InputRegistrar("General", "Escape", () => Mode.Value = PasteMode.None)
		};

		// Private
		private ChartSelectDataset dataset = default!;
		private Camera levelCamera = default!;
		private NotifiableProperty<ITimeRetriever> timeRetriever = default!;
		private NotifiableProperty<IWidthRetriever> widthRetriever = default!;

		private readonly List<ChartComponent> clipboard = new(); // Guarantees if Count > 0, currentPasteHandler != null
		private Dictionary<Type, IPasteHandler> pasteHandlers = default!;
		private IPasteHandler currentPasteHandler = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			ChartSelectDataset dataset,
			[Key("stage")] Camera levelCamera,
			NotifiableProperty<ITimeRetriever> timeRetriever,
			NotifiableProperty<IWidthRetriever> widthRetriever)
		{
			this.dataset = dataset;
			this.levelCamera = levelCamera;
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this).AsSelf();

		public bool CopyToClipboard()
		{
			if (dataset.CurrentSelecting.Value is null)
			{
				clipboard.Clear();
				return true;
			}

			bool isFindHandler = false;
			foreach (var pair in pasteHandlers)
			{
				if (pair.Key.IsInstanceOfType(dataset.CurrentSelecting.Value.Model))
				{
					isFindHandler = true;
					currentPasteHandler = pair.Value;
					break;
				}
			}

			if (!isFindHandler)
			{
				T3Logger.Log("Notice", "Edit_CopyPaste_CopyNotSupport", T3LogType.Info);
				return true;
			}

			T3Logger.Log("Notice", "Edit_CopyPaste_CopySuccess", T3LogType.Success);
			clipboard.Clear();
			clipboard.AddRange(dataset);
			return false;
		}

		public void Paste()
		{
			if (!levelCamera.ContainsScreenPoint(Input.mousePosition)) return;

			if (clipboard.Count == 0)
			{
				T3Logger.Log("Notice", "Edit_CopyPaste_Empty", T3LogType.Info);
				return;
			}

			bool isSuccess = currentPasteHandler.Paste(out var message);
			T3Logger.Log("Notice", message, isSuccess ? T3LogType.Success : T3LogType.Warn);
		}

		public void ExactPaste()
		{
			if (!levelCamera.ContainsScreenPoint(Input.mousePosition)) return;

			if (clipboard.Count == 0)
			{
				T3Logger.Log("Notice", "Edit_CopyPaste_Empty", T3LogType.Info);
				return;
			}

			bool isSuccess = currentPasteHandler.ExactPaste(out var message);
			T3Logger.Log("Notice", message, isSuccess ? T3LogType.Success : T3LogType.Warn);
		}

		public void CheckClipboard()
		{
			if (!EventManager.Instance.InvokeVeto("Edit_QueryCheckClipboard", out _)) return;
			string message = clipboard.Count == 0 ? "Edit_CopyPaste_Empty" : currentPasteHandler.GetDescription();
			T3Logger.Log("Notice", message, T3LogType.Info);
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			pasteHandlers = new()
			{
				[typeof(INote)] =
					new EditingNotePasteHandler(this, levelCamera, dataset, timeRetriever),
				[typeof(ITrack)] =
					new EditingTrackPasteHandler(this, levelCamera, dataset, timeRetriever, widthRetriever),
			};
		}
	}
}