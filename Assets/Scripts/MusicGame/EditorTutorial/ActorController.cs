#nullable enable

using System.Collections.Generic;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Movement;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using Yarn.Unity;

namespace MusicGame.EditorTutorial
{
	public class ActorController : HierarchySystem<ActorController>
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<string, GameObject> actorPrefabs = default!;
		[SerializeField] private Transform actorParent = default!;
		[SerializeField] private FloatMovementContainer movement = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			dialogueRunner.Registrar("anim", AnimateActor),
			dialogueRunner.Registrar("draw", SetIllustration),
			dialogueRunner.Registrar("destroy", DestroyActor),
		};

		// Private
		[Inject] private DialogueRunner dialogueRunner = default!;

		private Dictionary<string, GameObject> ActorPrefabs => actorPrefabs.Value;
		private readonly Dictionary<string, GameObject> actors = new();
		private float y = 0;

		// Defined Functions
		public async YarnTask AnimateActor(string name, string stateName, bool stop = false)
		{
			if (!TryGetOrCreateActor(name, out GameObject actor)) return;

			Animator animator = actor.GetComponent<Animator>();
			animator.Play(stateName, 0, 0f);
			animator.enabled = !stop;

			if (stop) return;
			await YarnTask.Yield();

			bool hasEnteredState = false;
			// ReSharper disable once LoopVariableIsNeverChangedInsideLoop
			while (true)
			{
				AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
				bool isCurrentState = stateInfo.IsName(stateName);

				if (isCurrentState)
				{
					hasEnteredState = true;
					if (stateInfo.normalizedTime >= 1f && !animator.IsInTransition(0)) break;
				}
				else if (hasEnteredState) break;

				await YarnTask.Yield();
			}

			animator.enabled = false;
		}

		public YarnTask SetIllustration(string name, string illustrationName)
		{
			if (!TryGetOrCreateActor(name, out GameObject actor)) return YarnTask.CompletedTask;
			actor.GetComponent<IllustrationCollection>().SetIllustration(illustrationName);
			var rect = actor.GetComponent<RectTransform>();
			y = rect.anchoredPosition.y;
			movement.Move(
				() => rect.anchoredPosition.y - y,
				value => rect.anchoredPosition = rect.anchoredPosition with { y = y + value });
			return YarnTask.CompletedTask;
		}

		public YarnTask DestroyActor(string name)
		{
			if (actors.Remove(name, out GameObject actor) && actor != null) Destroy(actor);
			return YarnTask.CompletedTask;
		}

		private bool TryGetOrCreateActor(string name, out GameObject actor)
		{
			if (actors.TryGetValue(name, out actor!) && actor != null)
			{
				return true;
			}

			if (ActorPrefabs.TryGetValue(name, out GameObject prefab) == false || prefab == null)
			{
				Debug.LogWarning($"Actor prefab not found for '{name}'.", this);
				actor = default!;
				return false;
			}

			actor = Instantiate(prefab, actorParent);
			actors[name] = actor;
			return true;
		}
	}
}