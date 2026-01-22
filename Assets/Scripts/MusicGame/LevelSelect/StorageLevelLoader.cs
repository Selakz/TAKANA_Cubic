#nullable enable

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.LevelSelect
{
	public class StorageLevelLoader : T3MonoBehaviour, ISelfInstaller
	{
		// Private
		private IDataset<LevelComponent<GameplayPreference>> dataset = default!;

		// Constructor
		[Inject]
		private void Construct(IDataset<LevelComponent<GameplayPreference>> dataset)
		{
			this.dataset = dataset;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Defined Functions
		public static async UniTask<List<RawLevelInfo<GameplayPreference>>> LoadLevels(string storagePath)
		{
			List<RawLevelInfo<GameplayPreference>> levels = new();
			if (!Directory.Exists(storagePath)) return levels;
			foreach (var folder in Directory.GetDirectories(storagePath))
			{
				var levelInfo = await RawLevelInfo<GameplayPreference>.FromFolder(folder);
				Debug.Log($"Try load level {folder} and {(levelInfo is null ? "failed" : "succeeded")}");
				if (levelInfo is not null) levels.Add(levelInfo);
			}

			return levels;
		}

		public static async UniTask UnzipLevels(string storagePath)
		{
			if (!Directory.Exists(storagePath))
			{
				Debug.LogError($"Storage path {storagePath} does not exist");
				return;
			}

			string[] files = Directory.GetFiles(storagePath);
			foreach (var file in files)
			{
				var fileInfo = new FileInfo(file);
				if (fileInfo.Extension == ".t3pkg")
				{
					var levelDirectory = fileInfo.FullName.Replace(fileInfo.Extension, string.Empty);
					bool extractSuccess = false;
					try
					{
						await Task.Run(() => ZipFile.ExtractToDirectory(fileInfo.FullName, levelDirectory));
						extractSuccess = true;
					}
					catch
					{
						Debug.LogError($"Failed to extract file {fileInfo.FullName}");
					}

					if (extractSuccess)
					{
						try
						{
							File.Delete(fileInfo.FullName);
						}
						catch
						{
							// TODO: Notify deletion failed
						}
					}
				}
			}
		}

		// System Functions
		async void Start()
		{
			var storagePath = ISingleton<LevelSetting>.Instance.StoragePath.Value;
			Directory.CreateDirectory(storagePath);
			await UnzipLevels(storagePath);
			var levels = await LoadLevels(storagePath);
			foreach (var level in levels) dataset.Add(new(level));
		}
	}
}