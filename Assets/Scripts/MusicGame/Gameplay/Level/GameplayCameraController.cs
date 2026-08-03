#nullable enable

using MusicGame.Gameplay.Stage;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Level
{
	public class GameplayCameraController : HierarchySystem<GameplayCameraController>
	{
		// Serializable and Public
		[SerializeField] private Camera gameCamera = default!;
		[SerializeField] private GameplayStageSkinConfig voezConfig = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<Vector3>(ISingleton<PlayfieldSetting>.Instance.CameraPosition, UpdateCamera),
			new PropertyRegistrar<Vector3>(ISingleton<PlayfieldSetting>.Instance.CameraRotation, UpdateCamera),
			new PropertyRegistrar<GameplayStageSkinConfig>(config, UpdateCamera)
		};

		// Private
		[Inject] private NotifiableProperty<GameplayStageSkinConfig> config = default!;

		private Vector3 initialPosition;

		// Defined Functions
		private void UpdateCamera()
		{
			var position = config.Value == voezConfig
				? Vector3.zero
				: ISingleton<PlayfieldSetting>.Instance.CameraPosition.Value;
			var rotation = config.Value == voezConfig
				? Vector3.zero
				: ISingleton<PlayfieldSetting>.Instance.CameraRotation.Value;
			gameCamera.transform.localPosition = initialPosition + position;
			gameCamera.transform.rotation = new Quaternion(rotation.x, rotation.y, rotation.z, 1f);
		}

		// System Functions
		protected override void OnEnable()
		{
			initialPosition = gameCamera.transform.localPosition;
			base.OnEnable();
		}
	}
}