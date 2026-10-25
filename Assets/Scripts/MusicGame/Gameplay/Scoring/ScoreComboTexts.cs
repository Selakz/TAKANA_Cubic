#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Scoring
{
	public class ScoreComboTexts : HierarchySystem<ScoreComboTexts>
	{
		// Serializable and Public
		[SerializeField] private string? key = null;
		[SerializeField] private TMP_Text scoreText = default!;
		[SerializeField] private TMP_Text comboText = default!;

		public override string? Key => key;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<double>(score, () => { scoreText.text = score.Value.ToString("0000000"); }),
			new PropertyRegistrar<int>(combo, () => { comboText.text = combo.Value.ToString(); })
		};

		// Private
		[Inject, Key("score")] private NotifiableProperty<double> score = default!;
		[Inject, Key("combo")] private NotifiableProperty<int> combo = default!;
	}
}