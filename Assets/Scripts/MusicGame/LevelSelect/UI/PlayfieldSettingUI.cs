#nullable enable

using MusicGame.Gameplay.Basic.T3;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;

namespace MusicGame.LevelSelect.UI
{
	public class PlayfieldSettingUI : HierarchySystem<PlayfieldSettingUI>
	{
		// Serializable and Public
		[SerializeField] private T3NoteViewPresenter[] noteSamples = default!;
		[SerializeField] private NotifiableDataContainer<float> thicknessContainer = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<float>(ISingleton<PlayfieldSetting>.Instance.NoteThicknessRatio,
				ratio => { thicknessContainer.Property.Value = ratio; }),
			new PropertyRegistrar<float>(thicknessContainer.Property, value =>
			{
				ISingleton<PlayfieldSetting>.Instance.NoteThicknessRatio.Value = value;
				ISingletonSetting<PlayfieldSetting>.SaveInstance();
			})
		};

		// System Functions
		void Start()
		{
			foreach (var sample in noteSamples)
			{
				foreach (var modifier in sample.ThicknessModifiers)
				{
					modifier.Register(v => new(v.x, v.y * ISingleton<PlayfieldSetting>.Instance.NoteThicknessRatio), 1);
				}
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			foreach (var sample in noteSamples)
			{
				foreach (var modifier in sample.ThicknessModifiers)
				{
					modifier.Unregister(1);
				}
			}
		}

		void Update()
		{
			foreach (var sample in noteSamples)
			{
				foreach (var modifier in sample.ThicknessModifiers)
				{
					modifier.Update();
				}
			}
		}
	}
}