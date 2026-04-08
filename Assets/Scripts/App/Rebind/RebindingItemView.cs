#nullable enable

using T3Framework.Runtime.I18N;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace App.Rebind
{
	public class RebindingItemView : MonoBehaviour
	{
		[field: SerializeField]
		public I18NTextBlock ActionNameText { get; set; } = default!;

		[field: SerializeField]
		public TextMeshProUGUI CurrentBindingText { get; set; } = default!;

		[field: SerializeField]
		public Button RebindButton { get; set; } = default!;
	}
}