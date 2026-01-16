#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.LevelResult.UI
{
	public class ResultButtons : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Button restartButton = default!;
		[SerializeField] private Button exitButton = default!;
		[SerializeField] private int playfieldSceneIndex;
		[SerializeField] private int levelSelectSceneIndex;

		// Private
		private NotifiableProperty<ResultInfo?> resultInfo = default!;

		// Constructor
		[Inject]
		private void Construct(NotifiableProperty<ResultInfo?> resultInfo)
		{
			this.resultInfo = resultInfo;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(restartButton, () =>
			{
				if (resultInfo.Value?.LevelInfo is { } levelInfo)
				{
					GameplayLevelLoader.SetLevelInfo(levelInfo);
					SceneManager.LoadScene(playfieldSceneIndex);
				}
				else
				{
					SceneManager.LoadScene(levelSelectSceneIndex);
				}
			}),
			new ButtonRegistrar(exitButton, () => SceneManager.LoadScene(levelSelectSceneIndex))
		};
	}
}