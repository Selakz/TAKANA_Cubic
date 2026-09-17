#nullable enable

using System.Collections.Generic;
using DG.Tweening;
using MusicGame.Gameplay.Performance;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Static;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.Utility
{
	public class DebugPanel : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Camera? camera = default!;
		[SerializeField] private Button? moveCameraButton = default!;
		[SerializeField] private TMP_Text? moveCameraLabel = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars
		{
			get
			{
				var list = new List<IEventRegistrar>
				{
					new PropertyRegistrar<bool>(ISingleton<PerformanceSetting>.Instance.UseDebugPanel,
						value => gameObject.SetActive(value))
				};
				if (camera != null && moveCameraButton != null && moveCameraLabel != null)
				{
					list.Add(new ButtonRegistrar(moveCameraButton, () =>
					{
						var start = camera.transform.position.x;
						if (cameraTweener is not null && cameraTweener.IsPlaying()) return;
						cameraTweener = DOTween.To(
							() => camera.transform.position.x,
							x => camera.transform.position = camera.transform.position with { x = x },
							isOut ? start + 50 : start - 50, 1f);
						isOut = !isOut;
						moveCameraLabel.text = isOut ? "In" : "Out";
					}));
				}

				return list.ToArray();
			}
		}

		// Private
		private bool isOut = false;
		private Tweener? cameraTweener = null;
	}
}