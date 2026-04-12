#nullable enable

using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.EditorTutorial
{
	public class IllustrationCollection : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Image targetImage = default!;
		[SerializeField] private InspectorDictionary<string, Sprite> illustrations = default!;

		// Defined Functions
		public void SetIllustration(string illustrationName)
		{
			targetImage.sprite = illustrations.Value[illustrationName];
		}

		// System Functions
		void OnValidate()
		{
			targetImage = GetComponent<Image>();
		}
	}
}