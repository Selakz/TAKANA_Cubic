using T3Framework.Runtime.Plugins;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Level.UI
{
	// TODO: extract a base class EventButton
	public class LoadLevelButton : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private EditorLevelLoader levelLoader;
		[SerializeField] private Button loadLevelButton;

		// Event Handlers
		private void OnLoadLevelButtonClick()
		{
			string folder = FileBrowser.OpenFolderDialog("选择工程文件夹");
			if (folder == null) return;
			levelLoader.LoadLevel(folder);
		}

		// System Functions
		void Awake()
		{
			loadLevelButton.onClick.AddListener(OnLoadLevelButtonClick);
		}
	}
}