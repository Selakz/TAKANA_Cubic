using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.Level;
using MusicGame.Components;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.MVC;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.Scoring.AutoScore
{
	public class HoldAutoScoreHandler : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Animator comboAnimator;

		// Private
		private IModelRetrievable modelRetrievable;

		private bool hasStartComboTriggered = false;
		private bool hasEndComboTriggered = false;

		private EditingHold Model
		{
			get
			{
				var id = modelRetrievable.GenericModel.Id;
				var component = IEditingChartManager.Instance.Chart[id];
				if (component is EditingHold editingHold)
				{
					return editingHold;
				}
				else
				{
					Debug.LogError($"{nameof(HoldAutoScoreHandler)} error finds a non-{nameof(EditingHold)}");
					return null;
				}
			}
		}

		// Static
		private const int AutoScorePriority = 50;
		private const float HoldHitSoundVolume = 1f;

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
			hasEndComboTriggered = chartTime > Model.TimeEnd;
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
					color => LevelManager.Instance.Music.ChartTime < Model.Hold.TimeEnd
						? Model.Note.Properties.Get("isDummy", false)
							? new Color(color.r, color.g, color.b,
								color.a * ISingletonSetting<AutoScoreSetting>.Instance.DummyNoteOpacity)
							: color
						: Color.clear,
					AutoScorePriority);
				modifiableView.PositionModifier.Register(
					value =>
					{
						if (LevelManager.Instance.Music.ChartTime <= Model.Hold.TimeEnd) return value;
						else
						{
							var endTimeX = Model.Note.Parent.Movement.GetPos(Model.Hold.TimeEnd);
							return new(endTimeX - Model.Note.Parent.Position.x, 0);
						}
					},
					AutoScorePriority);
			}

			if (modelRetrievable is IColliderView colliderView)
			{
				colliderView.ColliderEnabledModifier.Register(
					_ => LevelManager.Instance.Music.ChartTime <= Model.Hold.TimeEnd,
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

			if (modelRetrievable is IColliderView colliderView)
			{
				colliderView.ColliderEnabledModifier.Unregister(AutoScorePriority);
			}
		}

		void Update()
		{
			var time = LevelManager.Instance.Music.ChartTime;
			if (!Model.Note.Properties.Get("isDummy", false))
			{
				if (time > Model.Note.TimeJudge)
				{
					AutoScoreManager.Instance.AddCombo(Model.Id);
					if (!hasStartComboTriggered)
					{
						hasStartComboTriggered = true;
						EventManager.Instance.Invoke("Audio_OnPlayHitSound", HoldHitSoundVolume);
						EventManager.Instance.Invoke("AutoScore_OnPlayLaneBeam", Model.Parent.Id);
					}
				}

				if (time > Model.Hold.TimeEnd)
				{
					AutoScoreManager.Instance.AddCombo(-Model.Id);
					if (!hasEndComboTriggered)
					{
						hasEndComboTriggered = true;
						PlayComboAnimation();
						EventManager.Instance.Invoke("AutoScore_OnPlayLaneBeam", Model.Parent.Id);
					}
				}
			}
		}
	}
}