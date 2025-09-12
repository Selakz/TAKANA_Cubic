using MusicGame.Components.Notes.Movement;
using MusicGame.Components.Tracks;
using MusicGame.Gameplay;
using MusicGame.Gameplay.Level;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.MVC;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.Components.Notes
{
	/// <summary>
	/// Representing basic note: attached to a <see cref="ITrack"/>, should be hit on <see cref="TimeJudge"/>.
	/// </summary>
	public abstract class BaseNote : INote, IChildOf<ITrack>
	{
		/// <summary> Modifying id is allowed, but should be careful. </summary>
		public int Id { get; set; }

		public abstract GameObject Prefab { get; }

		public Vector3 Position { get; set; }

		/// <summary> The time to instantiate view layer object. </summary>
		public T3Time TimeInstantiate
		{
			get => Mathf.Max(Parent.TimeInstantiate, timeInstantiate);
			protected set => timeInstantiate = value;
		}

		public virtual T3Time TimeEnd
		{
			get => TimeJudge;
			set => TimeJudge = value;
		}

		public T3Time TimeJudge
		{
			get => timeJudge;
			set
			{
				timeJudge = value;
				movement.TimeJudge = timeJudge;
			}
		}

		public ITrack Parent
		{
			get => parent;
			set
			{
				parent = value;
				ResetTimeInstantiate();
			}
		}

		IComponent IComponent.Parent
		{
			get => Parent;
			set
			{
				if (value is not ITrack track)
				{
					Debug.LogError($"BaseNote's Parent should be {nameof(ITrack)}");
					return;
				}

				Parent = track;
			}
		}

		MonoBehaviour IControllerRetrievable.Controller => (this as IControllerRetrievable<MonoBehaviour>)?.Controller;

		public abstract bool IsPresent { get; }

		public INoteMovement Movement
		{
			get => movement;
			set
			{
				if (movement is not null) movement.OnMovementUpdated -= ResetTimeInstantiate;
				movement = value;
				movement.OnMovementUpdated += ResetTimeInstantiate;
				ResetTimeInstantiate();
			}
		}

		public JObject Properties { get; set; }

		// Private
		private T3Time timeInstantiate;
		private T3Time timeJudge;
		private ITrack parent;
		private INoteMovement movement;

		// Defined Functions
		protected BaseNote(T3Time timeJudge, ITrack parent)
		{
			Id = IComponent.GetUniqueId();
			this.parent = parent;
			Properties = new();
			this.timeJudge = timeJudge;
			Movement = new BaseNoteMoveList(timeJudge);
		}

		public abstract BaseNote Clone(T3Time timeJudge, ITrack belongingTrack);

		public abstract bool Generate();

		public abstract bool Destroy();

		protected virtual void ResetTimeInstantiate()
		{
			TimeInstantiate = Mathf.Min(
				Movement.FirstTimeWhen(ISingletonSetting<PlayfieldSetting>.Instance.UpperThreshold, true),
				Movement.FirstTimeWhen(ISingletonSetting<PlayfieldSetting>.Instance.LowerThreshold, false));
		}

		public virtual JToken GetSerializationToken()
		{
			// ReSharper disable once UseObjectOrCollectionInitializer
			var token = new JObject();
			token.Add("id", Id);
			token.Add("timeJudge", TimeJudge.Milli);
			token.Add("track", Parent.Id);
			token.AddIf("movement", Movement.Serialize(true),
				!(Movement is BaseNoteMoveList baseNoteMoveList && baseNoteMoveList.IsDefault()));
			token.AddIf("properties", Properties, Properties.Count > 0);
			return token;
		}
	}
}