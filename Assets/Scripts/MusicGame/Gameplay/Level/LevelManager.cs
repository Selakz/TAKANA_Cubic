#nullable enable

using System.Collections.Generic;
using MusicGame.ChartEditor.Level;
using MusicGame.Components;
using MusicGame.Components.Chart;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Speed;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;

namespace MusicGame.Gameplay.Level
{
	public class LevelManager : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private GameAudioPlayer music = default!;
		[SerializeField] private Camera levelCamera = default!;
		[SerializeField] private Transform poolingStorage = default!;

		// TODO: Refactor all who need levelInfo to use this container instead of calling instance
		[SerializeField] private NotifiableDataContainer<LevelInfo?> levelInfoContainer = default!;
		[SerializeField] private SpeedDataContainer speedContainer = default!;

		public static LevelManager Instance { get; private set; } = default!;

		public LevelInfo LevelInfo { get; private set; } = default!;

		public GameAudioPlayer Music => music;

		public Camera LevelCamera => levelCamera;

		public ISpeed LevelSpeed { get; private set; } = new T3Speed(1.0f);

		public Transform PoolingStorage => poolingStorage;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<LevelInfo?>(levelInfoContainer, (_, _) =>
			{
				var levelInfo = levelInfoContainer.Property.Value;
				if (levelInfo is null) return;

				LevelInfo = levelInfo; // TODO: Delete this
				if (levelInfo.Preference is EditorPreference preference)
				{
					speedContainer.Speed = preference.Speed;
				}

				if (componentEnumerator is not null)
				{
					componentEnumerator.Reset();
					while (componentEnumerator.MoveNext())
					{
						componentEnumerator.Current?.Destroy();
					}
				}

				componentEnumerator?.Dispose();
				componentEnumerator = LevelInfo.Chart.GetEnumerator();
				T3Time offset = LevelInfo.Chart.Properties.TryGetValue("offset", out var value)
					? T3Time.Parse(value.ToString())
					: 0;
				Music.Load(levelInfo.Music, offset);
				lastComponent = null;
			}),
			new DataContainerRegistrar<ISpeed>(speedContainer, (_, _) =>
			{
				// TODO: Delete this
				LevelSpeed = speedContainer.Property.Value;

				var levelInfo = levelInfoContainer.Property.Value;
				if (levelInfo?.Preference is EditorPreference preference)
				{
					preference.Speed = speedContainer.Property.Value.Speed;
				}

				// TODO: Delete this
				if (LevelInfo?.Preference is EditorPreference preference2)
				{
					preference2.Speed = speedContainer.Property.Value.Speed;
				}
			})
		};

		// Private
		private IEnumerator<IComponent>? componentEnumerator;
		private IComponent? lastComponent;

		// Defined Functions
		// TODO: Separate LevelManager -> LevelManager / StageManager / MusicPlayer 

		// Event Handlers
		private void LevelOnReset(T3Time chartTime)
		{
			Music.ChartTime = chartTime;
			componentEnumerator?.Reset();
			lastComponent = null;
		}

		private void LevelOnPause()
		{
			Music.Pause();
		}

		private void LevelOnResume()
		{
			Music.Play();
		}

		private void ChartOnUpdate(ChartInfo chartInfo)
		{
			LevelInfo.Chart = chartInfo;
			componentEnumerator?.Dispose();
			componentEnumerator = LevelInfo.Chart.GetEnumerator();
			lastComponent = null;
		}

		// System Functions
		void LateUpdate()
		{
			if ((lastComponent is null || lastComponent.TimeInstantiate < Music.ChartTime) &&
			    componentEnumerator is not null)
			{
				while (componentEnumerator.MoveNext())
				{
					lastComponent = componentEnumerator.Current;
					if (lastComponent is not null)
					{
						lastComponent.Generate();
						// At most generate one component whose timeInstantiate beyond current time
						if (lastComponent.TimeInstantiate > Music.ChartTime) break;
					}
				}
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			Instance = this;
			EventManager.Instance.AddListener<T3Time>("Level_OnReset", LevelOnReset);
			EventManager.Instance.AddListener("Level_OnPause", LevelOnPause);
			EventManager.Instance.AddListener("Level_OnResume", LevelOnResume);
			EventManager.Instance.AddListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			EventManager.Instance.RemoveListener<T3Time>("Level_OnReset", LevelOnReset);
			EventManager.Instance.RemoveListener("Level_OnPause", LevelOnPause);
			EventManager.Instance.RemoveListener("Level_OnResume", LevelOnResume);
			EventManager.Instance.RemoveListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
		}
	}
}