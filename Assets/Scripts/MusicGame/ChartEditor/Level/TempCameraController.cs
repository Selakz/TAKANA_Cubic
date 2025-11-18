#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.Level
{
	public class TempCameraController : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Camera gameCamera = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<Vector3>(ISingletonSetting<PlayfieldSetting>.Instance.CameraPosition,
				(_, _) =>
				{
					var value = ISingletonSetting<PlayfieldSetting>.Instance.CameraPosition.Value;
					gameCamera.transform.localPosition = initialPosition + value;
				}),
			new PropertyRegistrar<Vector3>(ISingletonSetting<PlayfieldSetting>.Instance.CameraRotation,
				(_, _) =>
				{
					var value = ISingletonSetting<PlayfieldSetting>.Instance.CameraRotation.Value;
					gameCamera.transform.rotation = new Quaternion(value.x, value.y, value.z, 1f);
				})
		};

		// Private
		private Vector3 initialPosition;

		// System Functions
		protected override void OnEnable()
		{
			initialPosition = gameCamera.transform.localPosition;
			base.OnEnable();
		}
	}
}