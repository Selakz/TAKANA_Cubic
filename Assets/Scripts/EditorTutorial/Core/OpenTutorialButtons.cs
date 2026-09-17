#nullable enable

using System.Linq;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Yarn.Unity;

namespace MusicGame.EditorTutorial
{
	public class OpenTutorialButtons : HierarchySystem<OpenTutorialButtons>
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<Button, string> tutorialButtons = default!;
		[SerializeField] private GameObject tutorialPanel = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new UnionRegistrar(() =>
			{
				ButtonRegistrar[] registrars = new ButtonRegistrar[tutorialButtons.Value.Count];
				int i = 0;
				foreach (var pair in tutorialButtons.Value)
				{
					registrars[i++] = new ButtonRegistrar(pair.Key, () => OpenTutorial(pair.Value));
				}

				return registrars.Cast<IEventRegistrar>();
			})
		};

		// Private
		[Inject] private readonly DialogueRunner dialogueRunner = default!;

		// Defined Functions
		private void OpenTutorial(string tutorialName)
		{
			tutorialPanel.SetActive(false);
			ISingleton<InputManager>.Instance.GlobalInputEnabled.Value = false;
			dialogueRunner.StartDialogue(tutorialName);
		}
	}
}