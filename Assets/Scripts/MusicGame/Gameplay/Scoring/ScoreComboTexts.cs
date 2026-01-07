#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Scoring
{
	public class ScoreComboTexts : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private TMP_Text scoreText = default!;
		[SerializeField] private TMP_Text comboText = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<double>(score, () => { scoreText.text = score.Value.ToString("0000000"); }),
			new PropertyRegistrar<int>(combo, () => { comboText.text = combo.Value.ToString(); })
		};

		// Private
		private NotifiableProperty<double> score = default!;
		private NotifiableProperty<int> combo = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			[Key("score")] NotifiableProperty<double> score,
			[Key("combo")] NotifiableProperty<int> combo)
		{
			this.score = score;
			this.combo = combo;
		}

		// IDK why VContainer don't inject this component if I don't explicitly declare this...
		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}