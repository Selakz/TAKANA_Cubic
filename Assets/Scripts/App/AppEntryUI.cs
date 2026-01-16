#nullable enable

using T3Framework.Runtime;
using T3Framework.Runtime.Movement;
using UnityEngine;
using UnityEngine.UI;

namespace App
{
	public class AppEntryUI : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private FloatMovementContainer breathingMovement = default!;
		[SerializeField] private FloatMovementContainer? rotatingMovement;
		[SerializeField] private Image breathingImage = default!;
		[SerializeField] private Image? iconImage;

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			breathingMovement.Move(
				() => breathingImage.color.a,
				value => breathingImage.color = breathingImage.color with { a = value }
			);
			if (iconImage != null && rotatingMovement != null)
			{
				rotatingMovement.Move(
					() => iconImage.transform.rotation.eulerAngles.z,
					value => iconImage.transform.rotation =
						Quaternion.Euler(iconImage.transform.rotation.eulerAngles with { z = value }));
			}
		}
	}
}