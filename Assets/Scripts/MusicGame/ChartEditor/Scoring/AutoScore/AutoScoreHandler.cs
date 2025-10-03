using MusicGame.ChartEditor.Level;
using MusicGame.Components;
using MusicGame.Components.Notes;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.MVC;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.Scoring.AutoScore
{
	public class AutoScoreHandler : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Animator comboAnimator;

		// Private
		private IModelRetrievable modelRetrievable;

		private bool hasStartComboTriggered = false;

		private EditingNote Model
		{
			get
			{
				var id = modelRetrievable.GenericModel.Id;
				var component = IEditingChartManager.Instance.Chart[id];
				if (component is EditingNote editingNote)
				{
					return editingNote;
				}
				else
				{
					Debug.LogError($"{nameof(AutoScoreHandler)} error finds a non-{nameof(EditingNote)}");
					return null;
				}
			}
		}

		// Static
		private const int AutoScorePriority = 50;
		private const float TapHitSoundVolume = 1f;
		private const float SlideHitSoundVolume = 0.5f;

		// Defined Functions
		private void StopComboAnimation()
		{
			comboAnimator.gameObject.SetActive(false);
		}

		private void PlayComboAnimation()
		{
			comboAnimator.gameObject.SetActive(true);
			comboAnimator.Play(0);
		}

		// Event Handlers
		private void LevelOnReset(T3Time chartTime)
		{
			hasStartComboTriggered = chartTime > Model.Note.TimeJudge;
			StopComboAnimation();
		}

		// System Functions
		void Awake()
		{
			modelRetrievable = transform.parent.GetComponent<IModelRetrievable>();
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener<T3Time>("Level_OnReset", LevelOnReset);
			StopComboAnimation();
			hasStartComboTriggered = LevelManager.Instance.Music.ChartTime > Model.Note.TimeJudge;
			if (modelRetrievable is IModifiableView2D modifiableView)
			{
				modifiableView.ColorModifier.Register(
					color => LevelManager.Instance.Music.ChartTime < Model.Note.TimeJudge
						? Model.Note.Properties.Get("isDummy", false)
							? new Color(color.r, color.g, color.b,
								color.a * ISingletonSetting<AutoScoreSetting>.Instance.DummyNoteOpacity)
							: color
						: Color.clear,
					AutoScorePriority);
				modifiableView.PositionModifier.Register(
					value =>
					{
						if (LevelManager.Instance.Music.ChartTime <= Model.Note.TimeJudge) return value;
						else
						{
							var judgeTimeX = Model.Note.Parent.Movement.GetPos(Model.Note.TimeJudge);
							return new(judgeTimeX - Model.Note.Parent.Position.x, 0);
						}
					},
					AutoScorePriority);
			}

			if (modelRetrievable is IColliderView2D colliderView)
			{
				colliderView.ColliderEnabledModifier.Register(
					_ => LevelManager.Instance.Music.ChartTime <= Model.Note.TimeJudge,
					AutoScorePriority);
			}
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<T3Time>("Level_OnReset", LevelOnReset);
			if (modelRetrievable is IModifiableView2D modifiableView)
			{
				modifiableView.ColorModifier.Unregister(AutoScorePriority);
				modifiableView.PositionModifier.Unregister(AutoScorePriority);
			}

			if (modelRetrievable is IColliderView2D colliderView)
			{
				colliderView.ColliderEnabledModifier.Unregister(AutoScorePriority);
			}
		}

		void Update()
		{
			var time = LevelManager.Instance.Music.ChartTime;
			if (!Model.Note.Properties.Get("isDummy", false) && time > Model.Note.TimeJudge)
			{
				AutoScoreManager.Instance.AddCombo(Model.Id);
				if (!hasStartComboTriggered)
				{
					hasStartComboTriggered = true;
					PlayComboAnimation();
					var volume = Model.Note is Tap ? TapHitSoundVolume : SlideHitSoundVolume;
					EventManager.Instance.Invoke("Audio_OnPlayHitSound", volume);
					EventManager.Instance.Invoke("AutoScore_OnPlayLaneBeam", Model.Parent.Id);
				}
			}
		}
	}
}