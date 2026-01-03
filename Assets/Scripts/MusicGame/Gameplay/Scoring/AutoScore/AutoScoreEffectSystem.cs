#nullable enable

using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.LaneBeam;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Static;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Scoring.AutoScore
{
	// Play hit sound and hit effect
	public class AutoScoreEffectSystem : T3System, ITickable
	{
		private readonly IViewPool<ChartComponent> viewPool;
		private readonly GameAudioPlayer music;
		private readonly ObjectPool<HitEffectAnimator> hitEffectPool;
		private readonly AudioSource hitSound;
		private readonly AutoScoreSystem system;
		private readonly AutoScoreViewSystem viewSystem;

		private T3Time lastFrameTime;
		private double lastHitTime;

		public AutoScoreEffectSystem(
			IObjectResolver resolver,
			[Key("stage")] IViewPool<ChartComponent> viewPool,
			GameAudioPlayer music,
			PrefabObject hitEffectPrefab,
			AudioSource hitSound,
			AutoScoreSystem system,
			AutoScoreViewSystem viewSystem) : base(true)
		{
			this.viewPool = viewPool;
			this.music = music;
			this.hitSound = hitSound;
			hitEffectPool = new ObjectPool<HitEffectAnimator>(
				() => hitEffectPrefab.Instantiate
					(resolver, viewSystem.NotePool.DefaultTransform, false).GetComponent<HitEffectAnimator>(),
				animator => animator.gameObject.SetActive(true),
				animator => animator.gameObject.SetActive(false),
				animator => Object.Destroy(animator.gameObject),
				defaultCapacity: 0);
			this.system = system;
			this.viewSystem = viewSystem;
			music.OnTimeJump += SkipOneFrame;
		}

		private void SkipOneFrame()
		{
			lastFrameTime = T3Time.MaxValue;
		}

		public void Tick()
		{
			foreach (var component in viewSystem.NotePool)
			{
				var model = (component.Model as INote)!;
				var comboInfo = system.Dataset.Value?[component];
				if (comboInfo is null) continue;
				foreach (var time in comboInfo.ComboTimes)
				{
					if (lastFrameTime < time && music.ChartTime >= time)
					{
						var animator = hitEffectPool.Get();
						Vector3 position = viewSystem.NotePool[component]!.transform.position;
						position.y = 0;
						position.z -= 0.01f;
						animator.transform.position = position;
						animator.PlayComboAnimation();
						AudioOnPlayHitSound(model);
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
				volume * ISingleton<EditorSetting>.Instance.HitSoundVolumePercent / 100f);
			lastHitTime = currentTime;
		}

		public override void Dispose()
		{
			base.Dispose();
			music.OnTimeJump -= SkipOneFrame;
		}
	}
}