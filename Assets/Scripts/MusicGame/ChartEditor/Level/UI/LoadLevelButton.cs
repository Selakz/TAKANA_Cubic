using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Plugins;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Level.UI
{
	public class LoadLevelButton : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Button loadLevelButton;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(loadLevelButton, OnLoadLevelButtonClick)
		};

		// Private
		private EditorLevelLoader levelLoader = default!;

		// Constructor
		[Inject]
		private void Construct(EditorLevelLoader levelLoader)
		{
			this.levelLoader = levelLoader;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private void OnLoadLevelButtonClick()
		{
#if !UNITY_ANDROID && !UNITY_IOS
			string folder = FileBrowser.OpenFolderDialog(I18NSystem.GetText("App_ChooseProject"));
			if (folder == null) return;
			levelLoader.LoadLevel(folder);
#endif
		}
	}
}