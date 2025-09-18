// Modified from Arcade-Plus: https://github.com/yojohanshinwataikei/Arcade-plus

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Command
{
	public class CommandManager : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Button undoButton;
		[SerializeField] private Button redoButton;
		[SerializeField] private uint bufferSize = 200;

		public static CommandManager Instance { get; private set; }

		// Private
		private readonly LinkedList<ICommand> undoList = new();
		private readonly LinkedList<ICommand> redoList = new();
		private ICommand preparing = null;

		// Static

		// Defined Functions
		public void Add(ICommand command)
		{
			if (command is EmptyCommand) return;

			if (preparing != null)
			{
				throw new Exception("有正在进行的命令，暂时不能执行新命令");
			}

			Debug.Log($"Do Command: {command.Name}");
			command.Do();
			undoList.AddLast(command);
			if (undoList.Count > bufferSize)
			{
				undoList.RemoveFirst();
			}

			redoList.Clear();
			UpdateState();
		}

		public void Undo()
		{
			//if (AdeOperationManager.Instance.HasOngoingOperation)
			//{
			//    AdeOperationManager.Instance.CancelOngoingOperation();
			//    return;
			//}
			if (preparing != null)
			{
				//AdeToast.Instance.Show("有正在进行的命令，暂时不能撤销");
				Debug.LogWarning("有正在进行的命令，暂时不能撤销");
				return;
			}

			if (undoList.Count == 0) return;
			ICommand cmd = undoList.Last.Value;
			undoList.RemoveLast();
			Debug.Log($"Undo Command: {cmd.Name}");
			cmd.Undo();
			redoList.AddLast(cmd);
			UpdateState();
		}

		public void Redo()
		{
			if (preparing != null)
			{
				//AdeToast.Instance.Show("有正在进行的命令，暂时不能重做");
				Debug.LogWarning("有正在进行的命令，暂时不能重做");
				return;
			}

			if (redoList.Count == 0) return;
			ICommand cmd = redoList.Last.Value;
			redoList.RemoveLast();
			Debug.Log($"Redo Command: {cmd.Name}");
			cmd.Do();
			undoList.AddLast(cmd);
			UpdateState();
		}

		public void Cancel()
		{
			if (preparing != null)
			{
				preparing.Undo();
				preparing = null;
			}
		}

		public void Clear()
		{
			Cancel();
			undoList.Clear();
			redoList.Clear();
			UpdateState();
		}

		public void SetBufferSize(uint size)
		{
			bufferSize = size;
			while (undoList.Count + redoList.Count > bufferSize)
			{
				if (redoList.Count > 0)
				{
					redoList.RemoveFirst();
				}
				else
				{
					undoList.RemoveFirst();
				}
			}

			UpdateState();
		}

		public void Prepare(ICommand command)
		{
			if (preparing != null)
			{
				throw new Exception("有正在进行的命令，暂时不能准备新命令");
			}

			preparing = command;
			preparing.Do();
		}

		public void Commit()
		{
			if (preparing != null)
			{
				Debug.Log($"执行命令: {preparing.Name}");
				undoList.AddLast(preparing);
				if (undoList.Count > bufferSize)
				{
					undoList.RemoveFirst();
				}

				redoList.Clear();
				preparing = null;
			}

			UpdateState();
		}

		private void UpdateState()
		{
			undoButton.interactable = undoList.Count > 0;
			redoButton.interactable = redoList.Count > 0;
		}

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			undoList.Clear();
			redoList.Clear();
			UpdateState();
		}

		// System Functions
		void OnEnable()
		{
			Instance = this;
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);

			InputManager.Instance.RegisterCanceled("EditorBasic", "Undo", _ => Undo());
			InputManager.Instance.RegisterCanceled("EditorBasic", "Redo", _ => Redo());
			UpdateState();
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}
	}
}