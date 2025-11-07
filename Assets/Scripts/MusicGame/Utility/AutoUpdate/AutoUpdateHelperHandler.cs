#nullable enable

using System.IO;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Message;
using Semver;
using T3Framework.Preset.DataContainers;
using T3Framework.Runtime;
using UnityEngine;

namespace MusicGame.Utility.AutoUpdate
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
				HeaderMessage.Show("自动更新工具损坏，重新下载制谱器以启用自动更新", HeaderMessage.MessageType.Error);
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
					HeaderMessage.Show("更新过程中发生错误", HeaderMessage.MessageType.Error);
					break;
				case AutoUpdateHelperHelper.UpdatePackResult.InvalidVersion:
					HeaderMessage.Show($"待安装的版本{version}无效", HeaderMessage.MessageType.Error);
					break;
				case AutoUpdateHelperHelper.UpdatePackResult.InvalidPackage:
					HeaderMessage.Show("待安装的制谱器文件损坏，请重新下载", HeaderMessage.MessageType.Error);
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
					HeaderMessage.Show($"v{version}版本制谱器下载成功！", HeaderMessage.MessageType.Success);
					break;
				case AutoUpdateHelperHelper.SavePackResult.ExecutionError:
					nextStatus = currentStatus switch
					{
						VersionStatus.NotChecked => VersionStatus.NotChecked,
						_ => VersionStatus.HasUpdateAndNotDownloaded
					};
					HeaderMessage.Show("保存新版本制谱器时出现错误", HeaderMessage.MessageType.Error);
					break;
				case AutoUpdateHelperHelper.SavePackResult.InvalidPackage:
					nextStatus = currentStatus switch
					{
						VersionStatus.NotChecked => VersionStatus.NotChecked,
						_ => VersionStatus.HasUpdateAndNotDownloaded
					};
					HeaderMessage.Show("下载的新版本制谱器文件已损坏，请重新下载", HeaderMessage.MessageType.Error);
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