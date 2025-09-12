using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.Message;
using MusicGame.ChartEditor.Select;
using MusicGame.Components;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.CopyPaste
{
	public class CopyPastePlugin : MonoBehaviour
	{
		// Serializable and Public
		public IEnumerable<IComponent> Clipboard => clipboard;

		// Private
		private readonly List<IComponent> clipboard = new(); // Guarantees when Count > 0, currentPasteHandler != null
		private Dictionary<Type, IPasteHandler> pasteHandlers;
		private IPasteHandler currentPasteHandler;

		// Static

		// Defined Functions
		public void CopyToClipboard()
		{
			List<IComponent> candidates = ISelectManager.Instance.SelectedTargets.Values.ToList();
			var enumerate = candidates.ToList();
			foreach (var candidate in enumerate.Where(candidate =>
				         !EventManager.Instance.InvokeVeto("Edit_QueryCopy", candidate, out _)))
			{
				candidates.Remove(candidate);
			}

			if (candidates.Count == 0) return;
			bool isFindHandler = false;
			foreach (var pair in pasteHandlers)
			{
				if (pair.Key.IsInstanceOfType(ISelectManager.Instance.CurrentSelecting))
				{
					isFindHandler = true;
					currentPasteHandler = pair.Value;
					break;
				}
			}

			if (!isFindHandler)
			{
				HeaderMessage.Show("暂不支持此类元件的复制", HeaderMessage.MessageType.Info);
				return;
			}

			HeaderMessage.Show("复制成功！", HeaderMessage.MessageType.Success);
			clipboard.Clear();
			clipboard.AddRange(candidates);

			EventManager.Instance.Invoke<object>("Edit_OnClearClipboard", this);
		}

		public void Paste()
		{
			if (!EventManager.Instance.InvokeVeto("Edit_QueryPaste", out _)) return;
			if (!LevelManager.Instance.LevelCamera.ContainsScreenPoint(Input.mousePosition))
			{
				return;
			}

			if (clipboard.Count == 0)
			{
				Debug.Log("clipboard empty");
				HeaderMessage.Show("剪贴板为空", HeaderMessage.MessageType.Info);
				return;
			}

			bool isSuccess = currentPasteHandler.Paste(out var message);
			HeaderMessage.Show(message, isSuccess ? HeaderMessage.MessageType.Success : HeaderMessage.MessageType.Warn);
		}

		public void ExactPaste()
		{
			if (!EventManager.Instance.InvokeVeto("Edit_QueryPaste", out _)) return;
			if (!LevelManager.Instance.LevelCamera.ContainsScreenPoint(Input.mousePosition))
			{
				return;
			}

			if (clipboard.Count == 0)
			{
				HeaderMessage.Show("剪贴板为空", HeaderMessage.MessageType.Info);
				return;
			}

			bool isSuccess = currentPasteHandler.ExactPaste(out var message);
			HeaderMessage.Show(message, isSuccess ? HeaderMessage.MessageType.Success : HeaderMessage.MessageType.Warn);
		}

		public void CheckClipboard()
		{
			if (!EventManager.Instance.InvokeVeto("Edit_QueryCheckClipboard", out _)) return;
			string message = clipboard.Count == 0 ? "剪贴板为空" : currentPasteHandler.GetDescription();
			HeaderMessage.Show(message, HeaderMessage.MessageType.Info);
		}

		// Event Handlers
		private void OnClearClipboard(object sender)
		{
			if (ReferenceEquals(sender, this)) return;
			clipboard.Clear();
		}

		// System Functions
		void Awake()
		{
			pasteHandlers = new()
			{
				[typeof(EditingNote)] = new EditingNotePasteHandler(this),
				[typeof(EditingTrack)] = new EditingTrackPasteHandler(this),
			};
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener<object>("Edit_OnClearClipboard", OnClearClipboard);

			InputManager.Instance.Register("InScreenEdit", "Cut", _ =>
			{
				CopyToClipboard();
				currentPasteHandler.Cut();
			});
			InputManager.Instance.Register("InScreenEdit", "Copy", _ => CopyToClipboard());
			InputManager.Instance.Register("InScreenEdit", "Paste", _ => Paste());
			InputManager.Instance.Register("InScreenEdit", "ExactPaste", _ => ExactPaste());
			InputManager.Instance.Register("InScreenEdit", "CheckClipboard", _ => CheckClipboard());
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<object>("Edit_OnClearClipboard", OnClearClipboard);
		}
	}
}