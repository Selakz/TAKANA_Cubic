#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Level.UI
{
	public class CoverLoader : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Texture2D defaultCover = default!;
		[SerializeField] private RawImage coverImage = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				var info = levelInfo.Value;
				var texture = info?.Cover ?? defaultCover;
				var textureRatio = (float)texture.width / texture.height;
				var rectRatio = coverImage.rectTransform.rect.width / coverImage.rectTransform.rect.height;
				var ratio = textureRatio / rectRatio;
				coverImage.texture = texture;
				coverImage.uvRect = ratio < 1
					? new Rect(0, (1 - ratio) / 2, 1, ratio)
					: new Rect((1 - 1 / ratio) / 2, 0, 1 / ratio, 1);
			})
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;

		// Defined Functions
		[Inject]
		private void Construct(NotifiableProperty<LevelInfo?> levelInfo)
		{
			this.levelInfo = levelInfo;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}