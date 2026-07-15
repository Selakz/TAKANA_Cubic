#nullable enable

using System;
using System.ComponentModel;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Setting;
using IComponent = T3Framework.Runtime.ECS.IComponent;

namespace MusicGame.Gameplay.Level
{
	public class PackInfo : ISetting<SongInfo>, IComponent
	{
		[Description("Id")]
		public string Id { get; set; } = string.Empty;

		[Description("Title")]
		public I18NString Title { get; set; } = new();

		[Description("Description")]
		public I18NString Description { get; set; } = new();

		[Description("CoverFileName")]
		public string CoverFileName { get; set; } = "cover.jpg";

		public event EventHandler? OnComponentUpdated;

		public void UpdateNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);

		public bool Contains(SongInfo song) => song.PackId == Id ||
		                                       ReferenceEquals(this, All) ||
		                                       (ReferenceEquals(this, Single) && string.IsNullOrEmpty(song.PackId));

		public static PackInfo All { get; } = new()
		{
			Title = I18NString.FromLocalized("Setting_PackInfo_All"),
		};

		public static PackInfo Single { get; } = new()
		{
			Title = I18NString.FromLocalized("Setting_PackInfo_Single"),
		};
	}
}