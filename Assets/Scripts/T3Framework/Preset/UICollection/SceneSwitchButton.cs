#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace T3Framework.Preset.UICollection
{
	public class SceneSwitchButton : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Button button = default!;
		[SerializeField] private int sceneIndex;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(button, () => SceneManager.LoadScene(sceneIndex))
		};
	}
}