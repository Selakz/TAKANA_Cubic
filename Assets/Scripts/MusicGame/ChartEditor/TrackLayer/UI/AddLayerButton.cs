#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLayer.UI
{
	public class AddLayerButton : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Button button = default!;
		[SerializeField] private TMP_InputField nameInputField = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(button, () =>
			{
				var layerName = nameInputField.text;
				if (string.IsNullOrEmpty(layerName)) return;
				if (manageSystem.LayersInfo.Value is { } info) info.AddNewLayer(layerName);
			})
		};

		// Private
		private TrackLayerManageSystem manageSystem = default!;

		// Constructor
		[Inject]
		private void Construct(
			TrackLayerManageSystem manageSystem)
		{
			this.manageSystem = manageSystem;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}