// Modified from Arcade-Plus: https://github.com/yojohanshinwataikei/Arcade-plus

#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Command
{
	public class CommandManager : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Button undoButton = default!;
		[SerializeField] private Button redoButton = default!;
		[SerializeField] private uint bufferSize = 200;

		public event Action<ICommand>? OnAdd;

		public event Action<ICommand>? OnUndo;

		/// <summary> Will be triggered also when first added. </summary>
		public event Action<ICommand>? OnRedo;

		public static CommandManager Instance { get; private set; } = default!; // TODO: Delete this

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("EditorBasic", "Undo", Undo),
			new InputRegistrar("EditorBasic", "Redo", Redo),
			new ButtonRegistrar(undoButton, Undo),
			new ButtonRegistrar(redoButton, Redo),
			new PropertyRegistrar<LevelInfo?>(levelInfo, Clear)
		};

		// Private
		private readonly LinkedList<ICommand> undoList = new();
		private readonly LinkedList<ICommand> redoList = new();
		private NotifiableProperty<LevelInfo?> levelInfo = default!;

		// Defined Functions
		[Inject]
		private void Construct(NotifiableProperty<LevelInfo?> levelInfo) => this.levelInfo = levelInfo;

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this).AsSelf();

		public void Add(ICommand command)
		{
			if (command.IsSkippable)
			{
				command.Do();
				OnAdd?.Invoke(command);
				OnRedo?.Invoke(command);
				return;
			}

			Debug.Log($"Do Command: {command.Name}");
			command.Do();
			undoList.AddLast(command);
			if (undoList.Count > bufferSize)
			{
				undoList.RemoveFirst();
			}

			redoList.Clear();
			OnAdd?.Invoke(command);
			OnRedo?.Invoke(command);
			UpdateState();
		}

		public void Undo()
		{
			if (undoList.Count == 0) return;
			ICommand cmd = undoList.Last.Value;
			undoList.RemoveLast();
			Debug.Log($"Undo Command: {cmd.Name}");
			cmd.Undo();
			redoList.AddLast(cmd);
			OnUndo?.Invoke(cmd);
			UpdateState();
		}

		public void Redo()
		{
			if (redoList.Count == 0) return;
			ICommand cmd = redoList.Last.Value;
			redoList.RemoveLast();
			Debug.Log($"Redo Command: {cmd.Name}");
			cmd.Do();
			undoList.AddLast(cmd);
			OnRedo?.Invoke(cmd);
			UpdateState();
		}

		public void Clear()
		{
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

		private void UpdateState() // TODO: Delete this
		{
			undoButton.interactable = undoList.Count > 0;
			redoButton.interactable = redoList.Count > 0;
		}

		// Event Handlers
		protected override void OnEnable()
		{
			base.OnEnable();
			Instance = this;
			UpdateState();
		}
	}
}