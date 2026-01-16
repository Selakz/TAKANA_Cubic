#nullable enable

using System;
using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using MusicGame.Gameplay.LaneBeam;
using MusicGame.Gameplay.Level;
using MusicGame.Gameplay.Scoring.AutoScore;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Scoring.JudgeScore
{
	public class JudgeScoreEffectSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private PrefabObject hitEffectPrefab = default!;
		[SerializeField] private Transform hitEffectRoot = default!;
		[SerializeField] private AudioSource hitSound = default!;
		[SerializeField] private InspectorDictionary<T3JudgeResult, Color> laneBeamColors = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<Action<IJudgeItem>>(
				e => judgeStorage.OnJudgeItemAdded += e,
				e => judgeStorage.OnJudgeItemAdded -= e,
				item =>
				{
					var component = item.ComboItem.FromComponent;
					if (item is IT3JudgeItem judgeItem &&
					    judgeItem.JudgeResult is not (T3JudgeResult.EarlyMiss or T3JudgeResult.LateMiss))
					{
						PlayHitEffect(judgeItem);
						if (component.Model is INote note) AudioOnPlayHitSound(note);
						PlayLaneBeam(judgeItem);
					}
				}),
		};

		// Private
		private IViewPool<ChartComponent> viewPool = default!;
		private JudgeStorage judgeStorage = default!;

		private ObjectPool<HitEffectAnimator> hitEffectPool = default!;
		private double lastHitTime;

		// Constructor
		[Inject]
		private void Construct(
			IObjectResolver resolver,
			[Key("stage")] IViewPool<ChartComponent> viewPool,
			JudgeStorage judgeStorage)
		{
			this.viewPool = viewPool;
			this.judgeStorage = judgeStorage;

			hitEffectPool = new ObjectPool<HitEffectAnimator>(
				() => hitEffectPrefab.Instantiate
					(resolver, hitEffectRoot, false).GetComponent<HitEffectAnimator>(),
				animator => animator.gameObject.SetActive(true),
				animator => animator.gameObject.SetActive(false),
				animator => Destroy(animator.gameObject),
				defaultCapacity: 10);
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Defined Functions
		private void PlayHitEffect(IT3JudgeItem judgeItem)
		{
			const float invalid = 10000;
			var x = judgeItem switch
			{
				HitJudgeItem hitJudgeItem => hitJudgeItem.TapPosition,
				HoldEndJudgeItem holdEndJudgeItem => holdEndJudgeItem.EndPosition,
				_ => invalid + 1
			};
			if (x >= invalid) return;
			var animator = hitEffectPool.Get();
			animator.transform.position = new(x, 0, -0.01f);
			animator.PlayComboAnimation();
			TempRelease(animator).Forget();
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

		private void PlayLaneBeam(IT3JudgeItem judgeItem)
		{
			var component = judgeItem.ComboItem.FromComponent;
			if (laneBeamColors.Value.TryGetValue(judgeItem.JudgeResult, out var color) &&
			    component.Parent is not null && viewPool[component.Parent] is var handler &&
			    handler?.GetPlugin("lane-beam")?.TryScript<LaneBeamPlugin>() is { } laneBeamPlugin)
			{
				laneBeamPlugin.LaneBeamColor = color;
				laneBeamPlugin.PlayAnimation();
			}
		}
	}
}