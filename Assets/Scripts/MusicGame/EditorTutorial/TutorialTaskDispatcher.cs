#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Audio;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using VContainer;
using Yarn.Unity;

namespace MusicGame.EditorTutorial
{
	public class TutorialTaskDispatcher : HierarchySystem<TutorialTaskDispatcher>
	{
		// Serializable and Public
		[SerializeField] private GameObject canvas = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			dialogueRunner.Registrar("task", Dispatch),
			dialogueRunner.Registrar("complete", Complete),
		};

		// Private
		[Inject] private DialogueRunner dialogueRunner = default!;
		[Inject] private IGameAudioPlayer audioPlayer = default!;

		private readonly Dictionary<string, List<Func<YarnTask>>> dispatchedTasks = new();

		// Defined Functions
		public async YarnTask Dispatch(string taskName, bool openForEdit = false)
		{
			if (!dispatchedTasks.TryGetValue(taskName, out List<Func<YarnTask>> tasks)) return;

			audioPlayer.Pause();
			canvas.SetActive(!openForEdit);
			ISingleton<InputManager>.Instance.GlobalInputEnabled.Value = openForEdit;
			await YarnTask.WhenAll(tasks.Select(task => task.Invoke()));

			canvas.SetActive(true);
			ISingleton<InputManager>.Instance.GlobalInputEnabled.Value = false;
		}

		public YarnTask Complete(string tutorialName)
		{
			var completedTutorials = ISingleton<TutorialInfo>.Instance.CompletedTutorials;
			if (!completedTutorials.Value.Contains(tutorialName)) completedTutorials.Value.Add(tutorialName);
			completedTutorials.ForceNotify();
			ISingletonSetting<TutorialInfo>.SaveInstance();
			ISingleton<InputManager>.Instance.GlobalInputEnabled.Value = true;
			ISingleton<InputManager>.Instance.AllowAll();
			return YarnTask.CompletedTask;
		}

		public IEventRegistrar Registrar(string taskName, Func<YarnTask> task)
		{
			return new TutorialTaskRegistrar(this, taskName, task);
		}

		private class TutorialTaskRegistrar : IEventRegistrar
		{
			private readonly TutorialTaskDispatcher dispatcher;
			private readonly string taskName;
			private readonly Func<YarnTask> task;

			public TutorialTaskRegistrar(TutorialTaskDispatcher dispatcher, string taskName, Func<YarnTask> task)
			{
				this.dispatcher = dispatcher;
				this.taskName = taskName;
				this.task = task;
			}

			public void Register()
			{
				if (!dispatcher.dispatchedTasks.ContainsKey(taskName)) dispatcher.dispatchedTasks[taskName] = new();
				dispatcher.dispatchedTasks[taskName].Add(task);
			}

			public void Unregister()
			{
				if (!dispatcher.dispatchedTasks.TryGetValue(taskName, out var tasks)) return;
				tasks.Remove(task);
				if (tasks.Count == 0) dispatcher.dispatchedTasks.Remove(taskName);
			}
		}
	}
}