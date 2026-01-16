#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Judge.T3;
using T3Framework.Runtime.Serialization.Inspector;
using TMPro;
using UnityEngine;

namespace MusicGame.LevelResult.UI
{
	public class JudgeDetailsView : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<T3JudgeResult, TextMeshProUGUI> judgeTexts = default!;
		[SerializeField] private string format = string.Empty;

		private readonly Dictionary<TextMeshProUGUI, int> textCounts = new();

		// Defined Functions
		public void SetValue(IReadOnlyDictionary<T3JudgeResult, int> judgeCounts)
		{
			textCounts.Clear();
			foreach (var (result, count) in judgeCounts)
			{
				if (!judgeTexts.Value.TryGetValue(result, out var text)) continue;
				textCounts.TryAdd(text, 0);
				textCounts[text] += count;
			}

			foreach (var (text, count) in textCounts)
			{
				text.text = count.ToString(format);
			}
		}
	}
}