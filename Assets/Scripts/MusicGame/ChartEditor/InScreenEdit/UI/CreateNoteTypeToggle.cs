#nullable enable

using System.Collections.Generic;
using MusicGame.Models;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit.UI
{
	public class CreateNoteTypeToggle : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<T3Flag, Toggle> toggles = default!;

		protected override IEventRegistrar[] EnableRegistrars => enableRegistrars;
		private IEventRegistrar[] enableRegistrars = default!;

		// Defined Functions
		[Inject]
		private void Construct(ChartEditInputSystem system)
		{
			List<IEventRegistrar> registrars = new();
			foreach (var pair in toggles.Value)
			{
				registrars.Add(new ToggleRegistrar(pair.Value, isOn =>
				{
					if (!isOn) return;
					system.NoteType.Value = pair.Key;
				}));
			}

			registrars.Add(new PropertyRegistrar<T3Flag>(system.NoteType, () =>
			{
				foreach (var pair in toggles.Value)
				{
					pair.Value.SetIsOnWithoutNotify(system.NoteType == pair.Key);
				}
			}));
			enableRegistrars = registrars.ToArray();
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}