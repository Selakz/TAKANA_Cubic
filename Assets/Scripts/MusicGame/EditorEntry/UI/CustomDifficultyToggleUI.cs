#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.EditorEntry.UI
{
	/// <summary> Component references on the runtime-generated custom difficulty row prefab. </summary>
	public class CustomDifficultyToggleUI : MonoBehaviour
	{
		[field: SerializeField] public Toggle Toggle { get; set; } = default!;

		[field: SerializeField] public TMP_InputField NameInputField { get; set; } = default!;

		[field: SerializeField] public TMP_InputField LevelInputField { get; set; } = default!;

		[field: SerializeField] public Image ColorIndicator { get; set; } = default!;

		[field: SerializeField] public Button ColorButton { get; set; } = default!;

		[field: SerializeField] public Button DeleteButton { get; set; } = default!;
	}
}
