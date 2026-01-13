#nullable enable

using System.IO;
using Cysharp.Threading.Tasks;
using Semver;
using T3Framework.Preset.DataContainers;
using T3Framework.Runtime;
using T3Framework.Runtime.Log;
using UnityEngine;

namespace App.AutoUpdate
{
	/// <summary> Encapsulate the WHOLE PROCESS of some operations about the helper. </summary>
	public class AutoUpdateHelperHandler : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private AutoUpdateConfig autoUpdateConfig = default!;
		[SerializeField] private VersionStatusDataContainer versionStatusDataContainer = default!;
		[SerializeField] private BoolDataContainer isInConfirmStateDataContainer = default!;

		// Private
		private SemVersion updaterVersion = default!;

		// Defined Functions
		private async UniTask<bool> CheckUpdaterValid()
		{
#if !UNITY_STANDALONE_WIN
			return true;
#endif
			if (!await AutoUpdateHelperHelper.UpdateUpdateHelper(updaterVersion))
			{
				T3Logger.Log("Notice", "AutoUpdate_UpdaterBroken", T3LogType.Error);
				return false;
			}

			return true;
		}

		public async UniTaskVoid TryInstall(string version, bool isForce)
		{
#if !UNITY_STANDALONE_WIN
			return;
#endif
			if (!await CheckUpdaterValid()) return;

			var result = await AutoUpdateHelperHelper.UpdatePack(version, isForce);
			switch (result)
			{
				case AutoUpdateHelperHelper.UpdatePackResult.Ready:
					Debug.LogError("ready to quit!");
					Application.Quit();
					break;
				case AutoUpdateHelperHelper.UpdatePackResult.ExecutionError:
					T3Logger.Log("Notice", "AutoUpdate_Updater_ExecutionError", T3LogType.Error);
					break;
				case AutoUpdateHelperHelper.UpdatePackResult.InvalidVersion:
					T3Logger.Log("Notice", $"AutoUpdate_Updater_InvalidVersion|{version}", T3LogType.Error);
					break;
				case AutoUpdateHelperHelper.UpdatePackResult.InvalidPackage:
					T3Logger.Log("Notice", "AutoUpdate_Updater_InvalidPackage", T3LogType.Error);
					break;
				case AutoUpdateHelperHelper.UpdatePackResult.ProjectFound:
					isInConfirmStateDataContainer.Property.Value = true;
					break;
			}
		}

		public async UniTaskVoid TrySavePack(string version, string packPath)
		{
#if !UNITY_STANDALONE_WIN
			return;
#endif
			if (!await CheckUpdaterValid())
			{
				versionStatusDataContainer.Property.Value = VersionStatus.NotChecked;
				return;
			}

			var result = await AutoUpdateHelperHelper.SavePack(version, packPath);
			var currentStatus = versionStatusDataContainer.Property.Value;
			var nextStatus = VersionStatus.HasUpdateAndHasDownloaded;
			switch (result)
			{
				case AutoUpdateHelperHelper.SavePackResult.Success:
					nextStatus = VersionStatus.HasUpdateAndHasDownloaded;
					T3Logger.Log("Notice", $"AutoUpdate_DownloadSuccess", T3LogType.Success);
					break;
				case AutoUpdateHelperHelper.SavePackResult.ExecutionError:
					nextStatus = currentStatus switch
					{
						VersionStatus.NotChecked => VersionStatus.NotChecked,
						_ => VersionStatus.HasUpdateAndNotDownloaded
					};
					T3Logger.Log("Notice", "AutoUpdate_SaveFail", T3LogType.Error);
					break;
				case AutoUpdateHelperHelper.SavePackResult.InvalidPackage:
					nextStatus = currentStatus switch
					{
						VersionStatus.NotChecked => VersionStatus.NotChecked,
						_ => VersionStatus.HasUpdateAndNotDownloaded
					};
					T3Logger.Log("Notice", "AutoUpdate_EditorBroken", T3LogType.Error);
					break;
			}

			if (File.Exists(packPath)) File.Delete(packPath);
			versionStatusDataContainer.Property.Value = nextStatus;
		}

		public async UniTask<bool> IsPackValid(string version)
		{
#if !UNITY_STANDALONE_WIN
			return true;
#endif
			if (!await CheckUpdaterValid()) return false;
			return await AutoUpdateHelperHelper.CheckPack(version) == AutoUpdateHelperHelper.CheckPackResult.Valid;
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			if (!SemVersion.TryParse(autoUpdateConfig.updateHelperVersion, out updaterVersion!))
			{
				Debug.LogError($"Updater version {autoUpdateConfig.updateHelperVersion} is not a valid version");
			}
		}
	}
}