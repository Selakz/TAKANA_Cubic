#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace MusicGame.Utility
{
	public class PlayfieldDebugOptions : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Button toggleCameraButton = default!;
		[SerializeField] private Camera mainCamera = default!;
		[SerializeField] private Button toggleBackgroundCameraButton = default!;
		[SerializeField] private Camera backgroundCamera = default!;
		[SerializeField] private Button toggleCoverButton = default!;
		[SerializeField] private GameObject coverCanvas = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(toggleCameraButton,
				() => mainCamera.transform.position = mainCamera.transform.position == defaultCameraPosition
					? new(10000, 10000, 0)
					: defaultCameraPosition),
			new ButtonRegistrar(toggleBackgroundCameraButton,
				() => backgroundCamera.gameObject.SetActive(!backgroundCamera.gameObject.activeSelf)),
			new ButtonRegistrar(toggleCoverButton,
				() => coverCanvas.gameObject.SetActive(!coverCanvas.gameObject.activeSelf))
		};

		// Private
		private Vector3 defaultCameraPosition;

		// System Functions
		protected override void Awake()
		{
			defaultCameraPosition = mainCamera.transform.position;
			mainCamera.opaqueSortMode = OpaqueSortMode.FrontToBack;
			base.Awake();
		}
	}
}