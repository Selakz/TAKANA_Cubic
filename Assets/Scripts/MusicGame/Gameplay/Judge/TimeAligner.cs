#nullable enable

using System;
using MusicGame.Gameplay.Audio;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Judge
{
	public class TimeAligner : T3MonoBehaviour, ISelfInstaller
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<Action>(
				e => music.OnPlay += e,
				e => music.OnPlay -= e,
				() => Align(music.ChartTime, AudioSettings.dspTime, Time.realtimeSinceStartupAsDouble)),
			CustomRegistrar.Generic<Action>(
				e => music.OnTimeJump += e,
				e => music.OnTimeJump -= e,
				() => Align(music.ChartTime, AudioSettings.dspTime, Time.realtimeSinceStartupAsDouble))
		};

		// Private
		private GameAudioPlayer music = default!;

		private float updatePace = 1;
		private T3Time chartTime;
		private double startDspTime;
		private double startUnityTime;
		private double pacedStartUnityTime;

		// Constructor
		[Inject]
		private void Construct(GameAudioPlayer music)
		{
			this.music = music;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Defined Functions
		/// <param name="unityTime"> the time fetched from <see cref="Time.realtimeSinceStartup"/>. </param>
		public void Align(T3Time chartTime, double dspTime, double unityTime)
		{
			this.chartTime = chartTime;
			startDspTime = dspTime;
			startUnityTime = unityTime;
			pacedStartUnityTime = unityTime;
			updatePace = 1;
		}

		public T3Time GetChartTime(double inputTime)
		{
			var time = chartTime.Second + (float)(inputTime - pacedStartUnityTime);
			return time;
		}

		// System Functions
		void Update()
		{
			// Trick from https://github.com/Arcthesia/ArcCreate
			var dspElapsed = AudioSettings.dspTime - startDspTime;
			var unityElapsed = Time.realtimeSinceStartupAsDouble - startUnityTime;
			updatePace = unityElapsed < 0 + Mathf.Epsilon
				? 1
				: Mathf.Lerp(updatePace, (float)(dspElapsed / unityElapsed), 0.1f);
			unityElapsed *= updatePace;
			pacedStartUnityTime = Time.realtimeSinceStartupAsDouble - unityElapsed;
		}
	}
}