#nullable enable

using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.LaneBeam;
using MusicGame.Gameplay.Level;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace MusicGame.Gameplay.Scoring.AutoScore
{
	// Play hit sound and hit effect
	public class AutoScoreEffectSystem : HierarchySystem<AutoScoreEffectSystem>
	{
		// Serializable and Public
		[SerializeField] private AudioSource hitSound = default!;
		[SerializeField] private PrefabObject hitEffectPrefab = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new CustomRegistrar(() => music.OnTimeJump += SkipOneFrame, () => music.OnTimeJump -= SkipOneFrame)
		};

		// Private
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;
		[Inject] private IGameAudioPlayer music = default!;
		[Inject] private AutoScoreSystem system = default!;

		private ObjectPool<HitEffectAnimator> hitEffectPool = default!;

		// Constructor
		[Inject]
		private void Construct(IObjectResolver resolver)
		{
			hitEffectPool = new ObjectPool<HitEffectAnimator>(
				() => hitEffectPrefab.Instantiate
					(resolver, viewPool.DefaultTransform, false).GetComponent<HitEffectAnimator>(),
				animator => animator.gameObject.SetActive(true),
				animator => animator.gameObject.SetActive(false),
				animator => Destroy(animator.gameObject),
				defaultCapacity: 0);
		}

		private T3Time lastFrameTime;
		private double lastHitTime;

		private void SkipOneFrame()
		{
			lastFrameTime = T3Time.MaxValue;
		}

		private async UniTaskVoid TempRelease(HitEffectAnimator animator)
		{
			await UniTask.Delay(1500);
			animator.StopComboAnimation();
			hitEffectPool.Release(animator);
		}

		private void AudioOnPlayHitSound(INote model)
		{
			float volume = model switch
			{
				Hit { Type: HitType.Slide } => 0.5f,
				_ => 1f
			};

			if (volume <= 0) return;
			double currentTime = AudioSettings.dspTime;
			if (currentTime - lastHitTime < 0.003) volume *= 0.5f;
			hitSound.PlayOneShot(hitSound.clip,
				volume * ISingleton<PlayfieldSetting>.Instance.HitSoundVolumePercent / 100f);
			lastHitTime = currentTime;
		}

		// System Functions
		public void Update()
		{
			foreach (var component in viewPool)
			{
				if (component.Model is not INote model) continue;
				var comboItems = system.Dataset.Value?[component];
				if (comboItems is null) continue;
				foreach (var item in comboItems)
				{
					if (lastFrameTime < item.ExpectedTime && music.ChartTime >= item.ExpectedTime)
					{
						var animator = hitEffectPool.Get();
						Vector3 position = viewPool[component]!.transform.position;
						position.y = 0;
						position.z -= 0.01f;
						animator.transform.position = position;
						animator.PlayComboAnimation();
						if (item.PlayHitSound) AudioOnPlayHitSound(model);
						TempRelease(animator).Forget();

						// LaneBeam
						if (component.Parent is not null && viewPool[component.Parent] is var handler)
						{
							var laneBeamPlugin = handler?.GetPlugin("lane-beam")?.TryScript<LaneBeamPlugin>();
							laneBeamPlugin?.PlayAnimation();
						}
					}
				}
			}

			lastFrameTime = music.ChartTime;
		}
	}
}