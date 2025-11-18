#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.Level.UI
{
	public class CameraSliders : T3MonoBehaviour
	{
		[SerializeField] private NotifiableDataContainer<float> cameraXDataContainer = default!;
		[SerializeField] private NotifiableDataContainer<float> cameraYDataContainer = default!;
		[SerializeField] private NotifiableDataContainer<float> cameraZDataContainer = default!;
		[SerializeField] private NotifiableDataContainer<float> cameraRotationDataContainer = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<Vector3>(ISingletonSetting<PlayfieldSetting>.Instance.CameraPosition, (_, _) =>
			{
				var position = ISingletonSetting<PlayfieldSetting>.Instance.CameraPosition.Value;
				cameraXDataContainer.Property.Value = position.x;
				cameraYDataContainer.Property.Value = position.y;
				cameraZDataContainer.Property.Value = position.z;
			}),
			new PropertyRegistrar<Vector3>(ISingletonSetting<PlayfieldSetting>.Instance.CameraRotation, (_, _) =>
			{
				var rotation = ISingletonSetting<PlayfieldSetting>.Instance.CameraRotation.Value;
				cameraRotationDataContainer.Property.Value = rotation.x;
			}),
			new DataContainerRegistrar<float>(cameraXDataContainer, (_, _) =>
			{
				var position = ISingletonSetting<PlayfieldSetting>.Instance.CameraPosition.Value;
				position.x = cameraXDataContainer.Property.Value;
				ISingletonSetting<PlayfieldSetting>.Instance.CameraPosition.Value = position;
				ISingletonSetting<PlayfieldSetting>.SaveInstance();
			}),
			new DataContainerRegistrar<float>(cameraYDataContainer, (_, _) =>
			{
				var position = ISingletonSetting<PlayfieldSetting>.Instance.CameraPosition.Value;
				position.y = cameraYDataContainer.Property.Value;
				ISingletonSetting<PlayfieldSetting>.Instance.CameraPosition.Value = position;
				ISingletonSetting<PlayfieldSetting>.SaveInstance();
			}),
			new DataContainerRegistrar<float>(cameraZDataContainer, (_, _) =>
			{
				var position = ISingletonSetting<PlayfieldSetting>.Instance.CameraPosition.Value;
				position.z = cameraZDataContainer.Property.Value;
				ISingletonSetting<PlayfieldSetting>.Instance.CameraPosition.Value = position;
				ISingletonSetting<PlayfieldSetting>.SaveInstance();
			}),
			new DataContainerRegistrar<float>(cameraRotationDataContainer, (_, _) =>
			{
				var rotation = ISingletonSetting<PlayfieldSetting>.Instance.CameraRotation.Value;
				rotation.x = cameraRotationDataContainer.Property.Value;
				ISingletonSetting<PlayfieldSetting>.Instance.CameraRotation.Value = rotation;
				ISingletonSetting<PlayfieldSetting>.SaveInstance();
			})
		};
	}
}