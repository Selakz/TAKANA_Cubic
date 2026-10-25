#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.I18N;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.Utility.UI
{
	/// <summary> Switches between the built-in difficulty view and the custom difficulty view. </summary>
	public class DifficultyViewSwitcher : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private GameObject builtInView = default!;
		[SerializeField] private GameObject customView = default!;
		[SerializeField] private Button switchButton = default!;
		[SerializeField] private TMP_Text? switchLabel;
		[SerializeField] private string builtInLabelId = "DifficultyView_BuiltIn";
		[SerializeField] private string customLabelId = "DifficultyView_Custom";

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(switchButton, () => Apply(!customView.activeSelf))
		};

		// Defined Functions
		/// <summary> Shows the custom view when the given difficulty is a custom one, otherwise the built-in view. </summary>
		public void ApplyForDifficulty(SongInfo? songInfo, int difficulty)
		{
			Apply(DifficultyCatalog.IsCustom(songInfo, difficulty));
		}

		public void Apply(bool showCustom)
		{
			builtInView.SetActive(!showCustom);
			customView.SetActive(showCustom);
			if (switchLabel != null)
				switchLabel.text = I18NSystem.GetText(showCustom ? builtInLabelId : customLabelId);
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			Apply(customView.activeSelf);
		}
	}
}
