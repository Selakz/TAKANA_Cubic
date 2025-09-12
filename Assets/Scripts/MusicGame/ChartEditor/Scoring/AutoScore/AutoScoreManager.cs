using System.Collections.Generic;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.Level;
using MusicGame.Components.Chart;
using MusicGame.Components.Notes;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MusicGame.ChartEditor.Scoring.AutoScore
{
	public class AutoScoreManager : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private GameObject handlerPrefab;
		[SerializeField] private GameObject holdHandlerPrefab;
		[SerializeField] private TMP_Text scoreText;
		[SerializeField] private TMP_Text comboText;

		public static AutoScoreManager Instance { get; private set; }

		// Private
		private readonly HashSet<int> comboIds = new();
		private int totalCount = 0;


		// Static
		private const double MaxScore = 1_000_000;

		private Dictionary<string, UnityAction<GameObject>> registerHandler;

		// Defined Functions
		private void AddComboInternal(int id)
		{
			comboIds.Add(id);
		}

		private void UpdateView()
		{
			comboText.text = comboIds.Count.ToString();
			scoreText.text = Mathf.RoundToInt((float)((double)comboIds.Count / totalCount * MaxScore))
				.ToString("0000000");
		}

		/// <summary> Hold's second combo's id is its id * -1 </summary>
		public void AddCombo(int id)
		{
			AddComboInternal(id);
			UpdateView();
		}

		private static int CalculateTotalCount(ChartInfo chartInfo)
		{
			int comboSum = 0;
			foreach (var component in chartInfo)
			{
				if (component is EditingNote editingNote && !editingNote.Note.Properties.Get("isDummy", false))
				{
					comboSum++;
					if (editingNote is EditingHold) comboSum++;
				}
			}

			return comboSum == 0 ? 1 : comboSum;
		}

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			comboIds.Clear();
			totalCount = CalculateTotalCount(levelInfo.Chart);
			UpdateView();
		}

		private void LevelOnReset(T3Time time)
		{
			comboIds.Clear();
			var chart = IEditingChartManager.Instance.Chart;
			foreach (var component in chart)
			{
				if (component is not EditingNote editingNote ||
				    editingNote.Note.Properties.Get("isDummy", false)) continue;
				if (editingNote.Note.TimeJudge <= time) AddComboInternal(editingNote.Id);
				if (editingNote.Note is Hold hold && hold.TimeEnd <= time) AddComboInternal(-hold.Id);
			}

			UpdateView();
		}

		private void ChartOnUpdate(ChartInfo chart)
		{
			comboIds.Clear();
			totalCount = CalculateTotalCount(chart);
			LevelOnReset(LevelManager.Instance.Music.ChartTime);
		}

		// System Functions
		void Awake()
		{
			registerHandler = new()
			{
				["TapPrefab_OnLoad"] = go => Instantiate(handlerPrefab, go.transform),
				["SlidePrefab_OnLoad"] = go => Instantiate(handlerPrefab, go.transform),
				["HoldPrefab_OnLoad"] = go => Instantiate(holdHandlerPrefab, go.transform),
				["LaneBeamPrefab_OnLoad"] = go => go.AddComponent<AutoScoreLaneBeamPlayer>(),
			};
		}

		void OnEnable()
		{
			Instance = this;
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
			EventManager.Instance.AddListener<T3Time>("Level_OnReset", LevelOnReset);
			EventManager.Instance.AddListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
			foreach (var pair in registerHandler)
			{
				// Currently not considering removing these components
				EventManager.Instance.AddListener(pair.Key, pair.Value);
			}
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
			EventManager.Instance.RemoveListener<T3Time>("Level_OnReset", LevelOnReset);
			EventManager.Instance.RemoveListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
		}
	}
}