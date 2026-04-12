#nullable enable

using System;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using Yarn.Unity;

namespace MusicGame.EditorTutorial
{
	public class TutorialInstaller : HierarchyInstaller
	{
		[SerializeField] private GameObject canvas = default!;
		[SerializeField] private DialogueRunner dialogueRunner = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterInstance(dialogueRunner);
		}

		private void SetCanvasActive() => canvas.SetActive(true);
		private void SetCanvasInactive() => canvas.SetActive(false);

		void OnEnable()
		{
			dialogueRunner.onDialogueStart?.AddListener(SetCanvasActive);
			dialogueRunner.onDialogueComplete?.AddListener(SetCanvasInactive);
		}

		void OnDisable()
		{
			dialogueRunner.onDialogueStart?.RemoveListener(SetCanvasActive);
			dialogueRunner.onDialogueComplete?.RemoveListener(SetCanvasInactive);
		}
	}

	public static class DialogueRunnerExtension
	{
		public static IEventRegistrar Registrar(
			this DialogueRunner dialogueRunner, string commandName, Delegate handler)
		{
			return new CustomRegistrar(
				() => dialogueRunner.AddCommandHandler(commandName, handler),
				() => dialogueRunner.RemoveCommandHandler(commandName));
		}
	}
}