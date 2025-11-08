#nullable enable

using MusicGame.Utility.AutoUpdate.Model;
using T3Framework.Runtime.Event;
using UnityEngine;

namespace MusicGame.Utility.AutoUpdate
{
	public enum VersionStatus
	{
		NotChecked,
		Latest,
		HasUpdateAndNotDownloaded,
		HasUpdateAndIsDownloading,
		HasUpdateAndHasDownloaded,
	}

	/// <summary>
	/// The caller should guarantee that when status is not <see cref="VersionStatus.NotChecked"/>
	/// or <see cref="VersionStatus.Latest"/>, <see cref="TargetVersionDescriptor"/> is always correct
	/// about the target version.
	/// </summary>
	public class VersionStatusDataContainer : NotifiableDataContainer<VersionStatus>
	{
		[SerializeField] private VersionStatus initialValue = VersionStatus.NotChecked;

		public override VersionStatus InitialValue => initialValue;

		public VersionDescriptor TargetVersionDescriptor { get; set; }
	}
}