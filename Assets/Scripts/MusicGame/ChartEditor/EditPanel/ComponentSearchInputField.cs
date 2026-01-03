#nullable enable

using System.Linq;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.EditPanel
{
	public class ComponentSearchInputField : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField searchInputField = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputFieldRegistrar(searchInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
				OnSearchInputFieldEndEdit),
			new PropertyRegistrar<LevelInfo?>(levelInfo, () => searchInputField.SetTextWithoutNotify(string.Empty))
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private ChartSelectDataset dataset = default!;

		// Defined Functions
		[Inject]
		private void Construct(NotifiableProperty<LevelInfo?> levelInfo, ChartSelectDataset dataset)
		{
			this.levelInfo = levelInfo;
			this.dataset = dataset;
		}

		// Event Handler
		private void OnSearchInputFieldEndEdit(string content)
		{
			if (string.IsNullOrWhiteSpace(content)) return;
			if (levelInfo.Value?.Chart is null) return;
			var chart = levelInfo.Value.Chart;

			dataset.Clear();
			bool isFind = false;
			if (int.TryParse(content, out int targetId))
			{
				foreach (var component in chart.Where(c => c.Id == targetId))
				{
					dataset.Add(component);
					isFind = true;
				}
			}

			foreach (var component in chart)
			{
				if (component.Name is not null && component.Name.Contains(content))
				{
					dataset.Add(component);
					isFind = true;
				}
			}

			if (!isFind)
			{
				T3Logger.Log("Notice", "Edit_SearchNotFound", T3LogType.Info);
			}
		}
	}
}