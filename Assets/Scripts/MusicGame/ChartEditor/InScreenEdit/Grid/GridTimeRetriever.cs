#nullable enable

using System;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class GridTimeRetriever : BaseTimeRetriever, IDisposable
	{
		// Serializable and Public
		public BpmList? BpmList { get; private set; }

		public NotifiableProperty<int> GridDivision { get; } = new(4) { Clamp = value => Mathf.Clamp(value, 1, 128) };

		// Private
		private readonly IEventRegistrar[] registrars;
		private readonly IGameAudioPlayer music;

		// Static

		// Defined Functions
		public GridTimeRetriever(
			NotifiableProperty<LevelInfo?> levelInfo,
			IGameAudioPlayer music)
		{
			this.music = music;
			registrars = new IEventRegistrar[]
			{
				new PropertyRegistrar<int>(GridDivision, () =>
				{
					if (levelInfo.Value?.Preference is EditorPreference preference)
					{
						preference.TimeGridLineCount = GridDivision.Value;
					}
				}),
				new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
				{
					var info = levelInfo.Value;
					if (info is null)
					{
						BpmList = null;
						return;
					}

					BpmList = info.GetsBpmList();
					if (info.Preference is EditorPreference preference)
					{
						GridDivision.Value = preference.TimeGridLineCount;
					}
				})
			};
			foreach (var registrar in registrars) registrar.Register();
		}

		public override T3Time GetTimeStart(Vector3 position)
		{
			if (BpmList is null) return base.GetTimeStart(position);

			T3Time time = GetCorrespondingTime(position.y);
			var ceilTime = BpmList.GetCeilTime(time, GridDivision, out var ceilIndex);
			var floorTime = BpmList.GetFloorTime(time, GridDivision, out var floorIndex);
			var ceilDistance = Mathf.Abs(ceilTime - time);
			var floorDistance = Mathf.Abs(floorTime - time);
			if (ceilIndex - floorIndex is 0 or 2)
				return time;
			if (ceilDistance > ISingletonSetting<InScreenEditSetting>.Instance.TimeSnapDistance.Value &&
			    floorDistance > ISingletonSetting<InScreenEditSetting>.Instance.TimeSnapDistance.Value)
				return time;
			return ceilDistance > floorDistance ? floorTime : ceilTime;
		}

		public override T3Time GetHoldTimeEnd(Vector3 position)
		{
			if (BpmList is null) return base.GetHoldTimeEnd(position);

			T3Time time = GetTimeStart(position);
			var ceilTime = BpmList.GetCeilTime(time, GridDivision, out _);
			return time == ceilTime ? BpmList.GetCeilTime(ceilTime + 1, GridDivision, out _) : ceilTime;
		}

		public override T3Time GetTrackTimeEnd(Vector3 position)
		{
			if (BpmList is null) return base.GetTrackTimeEnd(position);

			return ISingletonSetting<InScreenEditSetting>.Instance.IsInitialTrackLengthToEnd
				? music.AudioLength
				: BpmList!.GetCeilTime(
					GetCorrespondingTime(position.y) +
					ISingletonSetting<InScreenEditSetting>.Instance.InitialTrackLength,
					GridDivision, out _);
		}

		public T3Time GetCeilTime(T3Time time) => BpmList?.GetCeilTime(time, GridDivision, out _) ?? time;

		public T3Time GetFloorTime(T3Time time) => BpmList?.GetFloorTime(time, GridDivision, out _) ?? time;

		public void Dispose()
		{
			foreach (var registrar in registrars) registrar.Unregister();
		}
	}
}