#nullable enable

using System.IO;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using MusicGame.LevelSelect;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Static;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MusicGame.EditorEntry.UI
{
	public class ProjectItem : T3MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[field: SerializeField]
		public Image Icon { get; set; } = default!;

		[field: SerializeField]
		public TextMeshProUGUI NameText { get; set; } = default!;

		[field: SerializeField]
		public TextMeshProUGUI PathText { get; set; } = default!;

		[field: SerializeField]
		public GameObject SelectedFrame { get; set; } = default!;

		[field: SerializeField]
		public Button EnterButton { get; set; } = default!;

		[field: SerializeField]
		public Button DeleteButton { get; set; } = default!;

		// System Functions
		public void OnPointerEnter(PointerEventData eventData) => SelectedFrame.SetActive(true);

		public void OnPointerExit(PointerEventData eventData) => SelectedFrame.SetActive(false);
	}

	public class ProjectItemRegistrar : CompositeRegistrar
	{
		private readonly ProjectItem view;
		private readonly LevelComponent<EditorPreference> data;
		private readonly ListDataset<LevelComponent<EditorPreference>> dataset;
		private readonly NotifiableProperty<LevelInfo?> levelInfo;
		private readonly GameObject entryCanvas;

		protected override IEventRegistrar[] InnerRegistrars => new IEventRegistrar[]
		{
			new ComponentRegistrar(data, Initialize),
			new ButtonRegistrar(view.EnterButton, () =>
			{
				dataset.MoveToTop(data);
				entryCanvas.SetActive(false);
				ISingleton<InputManager>.Instance.GlobalInputEnabled.Value = true;
				data.Model.ToLevelInfo(0, ".editing.json", ".json").ContinueWith(info =>
				{
					if (info is not null) levelInfo.Value = info;
				});
			})
		};

		public ProjectItemRegistrar(
			ProjectItem view,
			LevelComponent<EditorPreference> data,
			ListDataset<LevelComponent<EditorPreference>> dataset,
			NotifiableProperty<LevelInfo?> levelInfo,
			GameObject entryCanvas)
		{
			this.view = view;
			this.data = data;
			this.dataset = dataset;
			this.levelInfo = levelInfo;
			this.entryCanvas = entryCanvas;
		}

		protected override void Initialize()
		{
			view.NameText.text = Path.GetFileName(data.Model.LevelPath);
			view.PathText.text = Path.GetDirectoryName(data.Model.LevelPath);
		}

		protected override void Deinitialize()
		{
			view.NameText.text = string.Empty;
			view.PathText.text = string.Empty;
		}
	}
}